using Microsoft.EntityFrameworkCore;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;
using InvalidOperationException = Predictly.Core.Exceptions.InvalidOperationException;

namespace Predictly.Infrastructure.Services;

public class MatchService(
    PredictlyDbContext db,
    IScoringService scoringService,
    ILeaderboardService leaderboardService) : IMatchService
{
    public async Task<MatchDto> CreateAsync(CreateMatchRequest request, CancellationToken ct = default)
    {
        _ = await db.Tournaments.FindAsync([request.TournamentId], ct)
            ?? throw new NotFoundException(nameof(Tournament), request.TournamentId);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            TournamentId = request.TournamentId,
            TeamA = request.TeamA,
            TeamB = request.TeamB,
            MatchStartTime = request.MatchStartTime,
            Status = "Scheduled"
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync(ct);
        return ToDto(match);
    }

    public async Task<MatchDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var match = await db.Matches.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Match), id);
        return ToDto(match);
    }

    public async Task<IReadOnlyList<MatchDto>> ListByTournamentAsync(Guid tournamentId, CancellationToken ct = default)
    {
        var matches = await db.Matches
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.MatchStartTime)
            .ToListAsync(ct);
        return matches.Select(ToDto).ToList();
    }

    /// <summary>
    /// Executes the match-completion flow from system design §6:
    ///   1. Validate match not already completed
    ///   2. Validate winner and bonus results
    ///   3. Persist bonus results
    ///   4. Score all predictions
    ///   5. Mark match completed
    ///   6. Rebuild leaderboards
    /// All steps 1–5 run inside a single DB transaction.
    /// </summary>
    public async Task CompleteAsync(Guid matchId, CompleteMatchRequest request, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var match = await db.Matches.FindAsync([matchId], ct)
            ?? throw new NotFoundException(nameof(Match), matchId);

        if (match.Status == "Completed")
            throw new InvalidOperationException($"Match '{matchId}' is already completed.");

        if (request.WinnerTeam != match.TeamA && request.WinnerTeam != match.TeamB)
            throw new InvalidOperationException($"Winner must be '{match.TeamA}' or '{match.TeamB}'.");

        // Persist bonus results
        var bonusResults = request.BonusResults.Select(r => new MatchBonusResult
        {
            MatchId = matchId,
            BonusQuestionCatalogId = r.BonusQuestionCatalogId,
            ActualValue = r.ActualValue
        });
        db.MatchBonusResults.AddRange(bonusResults);

        match.WinnerTeam = request.WinnerTeam;
        match.Status = "Completed";
        match.CompletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        // Score all predictions
        await scoringService.ScoreMatchAsync(matchId, ct);

        await tx.CommitAsync(ct);

        // Rebuild leaderboards outside transaction (heavy aggregation, idempotent)
        await leaderboardService.RebuildAsync(match.TournamentId, ct);
    }

    private static MatchDto ToDto(Match m) =>
        new(m.Id, m.TournamentId, m.TeamA, m.TeamB, m.MatchStartTime, m.Status, m.WinnerTeam);
}
