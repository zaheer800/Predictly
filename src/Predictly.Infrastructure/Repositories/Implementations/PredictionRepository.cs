using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

public class PredictionRepository : IPredictionRepository
{
    private readonly PredictlyDbContext _db;

    public PredictionRepository(PredictlyDbContext db) => _db = db;

    public Task<Prediction?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Predictions
           .Include(p => p.BonusAnswers)
           .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Prediction?> GetByUserAndMatchAsync(int userId, int matchId, CancellationToken ct = default) =>
        _db.Predictions
           .Include(p => p.BonusAnswers)
           .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId, ct);

    public Task<IReadOnlyList<Prediction>> GetByMatchIdAsync(int matchId, CancellationToken ct = default) =>
        _db.Predictions
           .Include(p => p.BonusAnswers)
           .Where(p => p.MatchId == matchId)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Prediction>)t.Result, ct);

    public async Task<Prediction> AddAsync(Prediction prediction, CancellationToken ct = default)
    {
        _db.Predictions.Add(prediction);
        await _db.SaveChangesAsync(ct);
        return prediction;
    }

    /// <summary>
    /// DB-level conditional update — only succeeds if now() &lt; match_start_time.
    /// Uses raw SQL to enforce the lock at database time, not application time.
    /// </summary>
    public async Task<bool> TryUpdateWithLockAsync(int predictionId, int userId, string predictedWinner, CancellationToken ct = default)
    {
        var rowsAffected = await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE predictions
            SET    predicted_winner = {predictedWinner},
                   finalized_at     = now()
            WHERE  id      = {predictionId}
              AND  user_id = {userId}
              AND  (SELECT match_start_time FROM matches WHERE id = match_id) > now()
            """,
            ct);

        return rowsAffected > 0;
    }
}
