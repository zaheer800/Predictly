using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Repositories.Interfaces;

public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Prediction?> GetByUserAndMatchAsync(int userId, int matchId, CancellationToken ct = default);
    Task<IReadOnlyList<Prediction>> GetByMatchIdAsync(int matchId, CancellationToken ct = default);
    Task<Prediction> AddAsync(Prediction prediction, CancellationToken ct = default);

    /// <summary>
    /// Updates predicted_winner and finalized_at only if now() &lt; match_start_time (DB-level lock).
    /// Returns false if the match is locked.
    /// </summary>
    Task<bool> TryUpdateWithLockAsync(int predictionId, int userId, string predictedWinner, CancellationToken ct = default);
}
