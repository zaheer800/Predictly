using Predictly.Application.DTOs.Admin;
using Predictly.Application.Interfaces;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class AdminUploadService : IAdminUploadService
{
    private readonly IExcelParser _excelParser;
    private readonly IMatchRepository _matchRepo;
    private readonly IMatchScoringService _scoringService;

    public AdminUploadService(
        IExcelParser excelParser,
        IMatchRepository matchRepo,
        IMatchScoringService scoringService)
    {
        _excelParser    = excelParser;
        _matchRepo      = matchRepo;
        _scoringService = scoringService;
    }

    public async Task<UploadSummaryResponse> ProcessUploadAsync(
        Stream fileStream, int uploadedBy, CancellationToken ct = default)
    {
        var parsed          = _excelParser.Parse(fileStream);
        var warnings        = new List<string>();
        var errors          = new List<string>(parsed.ParseErrors);
        var matchesCreated  = 0;
        var matchesUpdated  = 0;
        var resultsProcessed = 0;

        // ── Process MatchSetup rows ───────────────────────────────────────────
        foreach (var row in parsed.MatchSetups)
        {
            try
            {
                var exists = await _matchRepo.ExistsByKeyAsync(
                    row.TournamentId, row.TeamHome, row.TeamAway, row.MatchStartTime, ct);

                if (exists)
                {
                    warnings.Add($"Match {row.TeamHome} vs {row.TeamAway} at {row.MatchStartTime:u} already exists — skipped.");
                    continue;
                }

                var match = new Match
                {
                    TournamentId   = row.TournamentId,
                    TeamHome       = row.TeamHome,
                    TeamAway       = row.TeamAway,
                    Venue          = row.Venue,
                    MatchStartTime = row.MatchStartTime,
                    CreatedBy      = uploadedBy,
                    CreatedAt      = DateTime.UtcNow,
                    UpdatedAt      = DateTime.UtcNow
                };

                await _matchRepo.AddAsync(match, ct);
                matchesCreated++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to create match {row.TeamHome} vs {row.TeamAway}: {ex.Message}");
            }
        }

        // ── Process MatchResults rows ─────────────────────────────────────────
        foreach (var row in parsed.MatchResults)
        {
            try
            {
                var match = await _matchRepo.GetForScoringAsync(row.MatchId, ct);
                if (match is null)
                {
                    errors.Add($"Match {row.MatchId} not found — results skipped.");
                    continue;
                }

                // Set winner and bonus results on the match
                match.Winner    = row.Winner;
                match.UpdatedAt = DateTime.UtcNow;

                foreach (var bonusResult in row.BonusResults)
                {
                    match.BonusResults.Add(new MatchBonusResult
                    {
                        MatchId               = match.Id,
                        MatchBonusSelectionId = bonusResult.SelectionId,
                        ActualNumeric         = bonusResult.ActualNumeric,
                        ActualChoice          = bonusResult.ActualChoice,
                        SubmittedBy           = uploadedBy,
                        SubmittedAt           = DateTime.UtcNow
                    });
                }

                await _matchRepo.UpdateAsync(match, ct);
                matchesUpdated++;

                // Trigger scoring for this match
                await _scoringService.ScoreMatchAsync(row.MatchId, ct);
                resultsProcessed++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to process result for match {row.MatchId}: {ex.Message}");
            }
        }

        return new UploadSummaryResponse(
            matchesCreated,
            matchesUpdated,
            resultsProcessed,
            warnings,
            errors);
    }
}
