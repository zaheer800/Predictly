using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

public class PredictionScoreRepository : IPredictionScoreRepository
{
    private readonly PredictlyDbContext _db;

    public PredictionScoreRepository(PredictlyDbContext db) => _db = db;

    public async Task BulkInsertAsync(IEnumerable<PredictionScore> scores, CancellationToken ct = default)
    {
        _db.PredictionScores.AddRange(scores);
        await _db.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<PredictionScore>> GetByMatchIdAsync(int matchId, CancellationToken ct = default) =>
        _db.PredictionScores
           .Where(s => s.MatchId == matchId)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<PredictionScore>)t.Result, ct);
}
