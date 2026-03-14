using Microsoft.EntityFrameworkCore;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Infrastructure.Services;

public class PredictionService(PredictlyDbContext db) : IPredictionService
{
    public async Task<PredictionDto> UpsertAsync(
        Guid userId,
        UpsertPredictionRequest request,
        CancellationToken ct = default)
    {
        // Lock enforcement: database now() is authoritative (§8)
        var rowsAffected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE matches
            SET id = id  -- no-op touch; real enforcement is the WHERE clause
            WHERE id = {request.MatchId}
              AND status = 'Scheduled'
              AND match_start_time > now()
            """, ct);

        if (rowsAffected == 0)
        {
            // Either match doesn't exist or lock has triggered
            var match = await db.Matches.FindAsync([request.MatchId], ct);
            if (match is null)
                throw new NotFoundException(nameof(Match), request.MatchId);
            throw new PredictionLockedException(request.MatchId);
        }

        var existing = await db.Predictions
            .Include(p => p.BonusAnswers)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == request.MatchId, ct);

        if (existing is null)
        {
            var prediction = new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MatchId = request.MatchId,
                PredictedWinner = request.PredictedWinner,
                FinalizedAt = DateTimeOffset.UtcNow,
                BonusAnswers = request.BonusAnswers
                    .Select(a => new PredictionBonusAnswer
                    {
                        BonusQuestionCatalogId = a.BonusQuestionCatalogId,
                        AnswerValue = a.AnswerValue
                    })
                    .ToList()
            };
            db.Predictions.Add(prediction);
            await db.SaveChangesAsync(ct);
            return ToDto(prediction);
        }

        // Update existing prediction
        existing.PredictedWinner = request.PredictedWinner;
        existing.FinalizedAt = DateTimeOffset.UtcNow;

        db.PredictionBonusAnswers.RemoveRange(existing.BonusAnswers);
        existing.BonusAnswers = request.BonusAnswers
            .Select(a => new PredictionBonusAnswer
            {
                PredictionId = existing.Id,
                BonusQuestionCatalogId = a.BonusQuestionCatalogId,
                AnswerValue = a.AnswerValue
            })
            .ToList();

        await db.SaveChangesAsync(ct);
        return ToDto(existing);
    }

    public async Task<PredictionDto?> GetByUserAndMatchAsync(Guid userId, Guid matchId, CancellationToken ct = default)
    {
        var prediction = await db.Predictions
            .Include(p => p.BonusAnswers)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId, ct);

        return prediction is null ? null : ToDto(prediction);
    }

    public async Task<IReadOnlyList<PredictionDto>> ListByMatchAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await db.Matches.FindAsync([matchId], ct)
            ?? throw new NotFoundException(nameof(Match), matchId);

        // Predictions only visible after match completion (§2.1)
        if (match.Status != "Completed")
            return [];

        var predictions = await db.Predictions
            .Include(p => p.BonusAnswers)
            .Where(p => p.MatchId == matchId)
            .ToListAsync(ct);

        return predictions.Select(ToDto).ToList();
    }

    private static PredictionDto ToDto(Prediction p) => new(
        p.Id,
        p.UserId,
        p.MatchId,
        p.PredictedWinner,
        p.BonusAnswers.Select(a => new BonusAnswerDto(a.BonusQuestionCatalogId, a.AnswerValue)).ToList(),
        p.FinalizedAt);
}
