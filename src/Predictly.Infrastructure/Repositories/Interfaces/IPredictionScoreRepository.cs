using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Repositories.Interfaces;

public interface IPredictionScoreRepository
{
    Task BulkInsertAsync(IEnumerable<PredictionScore> scores, CancellationToken ct = default);
    Task<IReadOnlyList<PredictionScore>> GetByMatchIdAsync(int matchId, CancellationToken ct = default);
}
