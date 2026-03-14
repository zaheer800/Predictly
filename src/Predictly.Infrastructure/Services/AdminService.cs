using Microsoft.EntityFrameworkCore;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;
using InvalidOperationException = Predictly.Core.Exceptions.InvalidOperationException;

namespace Predictly.Infrastructure.Services;

public class AdminService(PredictlyDbContext db) : IAdminService
{
    public async Task<IReadOnlyList<BonusQuestionDto>> ListBonusCatalogAsync(CancellationToken ct = default)
    {
        var catalog = await db.BonusQuestionCatalog.OrderBy(b => b.Id).ToListAsync(ct);
        return catalog.Select(ToDto).ToList();
    }

    public async Task<BonusQuestionDto> GetBonusQuestionAsync(int id, CancellationToken ct = default)
    {
        var question = await db.BonusQuestionCatalog.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(BonusQuestionCatalog), id);
        return ToDto(question);
    }

    /// <summary>
    /// Sets the allowed bonus question types for a tournament.
    /// Throws if the tournament is already Active or Completed (immutable after activation).
    /// </summary>
    public async Task SetTournamentBonusConfigAsync(
        Guid tournamentId,
        SetBonusConfigRequest request,
        CancellationToken ct = default)
    {
        var tournament = await db.Tournaments
            .Include(t => t.BonusConfigs)
            .FirstOrDefaultAsync(t => t.Id == tournamentId, ct)
            ?? throw new NotFoundException(nameof(Tournament), tournamentId);

        if (tournament.Status != "Draft")
            throw new InvalidOperationException(
                $"Bonus configuration cannot be changed once tournament is '{tournament.Status}'.");

        // Validate all requested IDs exist in the catalog
        var validIds = await db.BonusQuestionCatalog
            .Where(b => request.BonusQuestionCatalogIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(ct);

        var missing = request.BonusQuestionCatalogIds.Except(validIds).ToList();
        if (missing.Count > 0)
            throw new NotFoundException(nameof(BonusQuestionCatalog), string.Join(", ", missing));

        // Replace existing config
        db.TournamentBonusConfigs.RemoveRange(tournament.BonusConfigs);
        tournament.BonusConfigs = request.BonusQuestionCatalogIds
            .Select(id => new TournamentBonusConfig
            {
                TournamentId = tournamentId,
                BonusQuestionCatalogId = id
            })
            .ToList();

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<BonusQuestionDto>> GetTournamentBonusConfigAsync(
        Guid tournamentId, CancellationToken ct = default)
    {
        _ = await db.Tournaments.FindAsync([tournamentId], ct)
            ?? throw new NotFoundException(nameof(Tournament), tournamentId);

        return await db.TournamentBonusConfigs
            .Where(c => c.TournamentId == tournamentId)
            .Select(c => c.BonusQuestion)
            .Select(b => new BonusQuestionDto(b.Id, b.QuestionText, b.QuestionType, b.Options, b.MaxPoints))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Randomly selects 3 bonus questions from the tournament's configured pool
    /// and assigns them to the match.
    /// </summary>
    public async Task AssignMatchBonusQuestionsAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await db.Matches
            .Include(m => m.BonusSelections)
            .FirstOrDefaultAsync(m => m.Id == matchId, ct)
            ?? throw new NotFoundException(nameof(Match), matchId);

        if (match.BonusSelections.Count > 0)
            throw new InvalidOperationException($"Match '{matchId}' already has bonus questions assigned.");

        var eligibleQuestions = await db.TournamentBonusConfigs
            .Where(c => c.TournamentId == match.TournamentId)
            .Select(c => c.BonusQuestionCatalogId)
            .ToListAsync(ct);

        if (eligibleQuestions.Count < 3)
            throw new InvalidOperationException(
                "Tournament must have at least 3 bonus questions configured to assign to a match.");

        // Deterministic random using Guid to avoid per-request seed issues
        var selected = eligibleQuestions.OrderBy(_ => Guid.NewGuid()).Take(3).ToList();

        var selections = selected.Select((questionId, index) => new MatchBonusSelection
        {
            MatchId = matchId,
            BonusQuestionCatalogId = questionId,
            DisplayOrder = index + 1
        });

        db.MatchBonusSelections.AddRange(selections);
        await db.SaveChangesAsync(ct);
    }

    private static BonusQuestionDto ToDto(BonusQuestionCatalog b) =>
        new(b.Id, b.QuestionText, b.QuestionType, b.Options, b.MaxPoints);
}
