using Predictly.Application.Interfaces;
using Predictly.Domain.Enums;
using Predictly.Domain.Exceptions;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

/// <summary>
/// Orchestrates the full scoring flow as defined in CLAUDE.md §8.5.
/// Steps 2–9 are atomic (wrapped in a DB transaction).
/// Step 10 (leaderboard rebuild) is separate and idempotent.
/// </summary>
public class MatchScoringService : IMatchScoringService
{
    // DbContext is injected ONLY for cross-repo transaction management (per CLAUDE.md §5.1:
    // services must not use DbContext for data access — repositories handle all queries).
    private readonly PredictlyDbContext _db;
    private readonly IMatchRepository _matchRepo;
    private readonly IPredictionRepository _predictionRepo;
    private readonly IScoringEngine _scoringEngine;
    private readonly IPredictionScoreRepository _scoreRepo;
    private readonly ILeaderboardRepository _leaderboardRepo;

    public MatchScoringService(
        PredictlyDbContext db,
        IMatchRepository matchRepo,
        IPredictionRepository predictionRepo,
        IScoringEngine scoringEngine,
        IPredictionScoreRepository scoreRepo,
        ILeaderboardRepository leaderboardRepo)
    {
        _db = db;
        _matchRepo = matchRepo;
        _predictionRepo = predictionRepo;
        _scoringEngine = scoringEngine;
        _scoreRepo = scoreRepo;
        _leaderboardRepo = leaderboardRepo;
    }

    public async Task ScoreMatchAsync(int matchId, CancellationToken ct = default)
    {
        int tournamentId;

        // Steps 2–9: atomic transaction
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var match = await _matchRepo.GetForScoringAsync(matchId, ct)
                ?? throw new NotFoundException(nameof(Domain.Entities.Match), matchId);

            // Step 3: validate match is not already completed
            if (match.Status == MatchStatus.Completed)
                throw new BusinessRuleException("MATCH_ALREADY_SCORED", $"Match {matchId} has already been scored.");

            if (string.IsNullOrWhiteSpace(match.Winner))
                throw new BusinessRuleException("MISSING_WINNER", $"Match {matchId} has no winner set.");

            // Step 4: validate bonus results exist
            if (match.BonusResults.Count != match.BonusSelections.Count)
                throw new BusinessRuleException("INCOMPLETE_BONUS_RESULTS", "All bonus question results must be submitted before scoring.");

            // Step 5: load all predictions with bonus answers
            var predictions = await _predictionRepo.GetByMatchIdAsync(matchId, ct);

            // Step 6: compute scores
            var scores = predictions
                .Select(p => _scoringEngine.ComputePredictionScore(
                    p,
                    match.Winner,
                    match.BonusSelections.ToList(),
                    match.BonusResults.ToList(),
                    match.TournamentId))
                .ToList();

            // Step 7: bulk insert prediction_scores
            await _scoreRepo.BulkInsertAsync(scores, ct);

            // Step 8: mark match completed
            match.Status = MatchStatus.Completed;
            match.UpdatedAt = DateTime.UtcNow;
            await _matchRepo.UpdateAsync(match, ct);

            tournamentId = match.TournamentId;

            // Step 9: commit
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        // Step 10: leaderboard rebuild — separate, idempotent, can be retried independently
        await _leaderboardRepo.RebuildTournamentLeaderboardAsync(tournamentId, ct);
        await _leaderboardRepo.RebuildGlobalLeaderboardAsync(ct);
    }
}
