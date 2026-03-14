using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface IPredictionService
{
    /// <summary>
    /// Creates or updates a prediction. Enforces lock at match_start_time.
    /// </summary>
    Task<PredictionDto> UpsertAsync(Guid userId, UpsertPredictionRequest request, CancellationToken ct = default);

    Task<PredictionDto?> GetByUserAndMatchAsync(Guid userId, Guid matchId, CancellationToken ct = default);

    /// <summary>
    /// Returns all predictions for a match. Only visible after match completion.
    /// </summary>
    Task<IReadOnlyList<PredictionDto>> ListByMatchAsync(Guid matchId, CancellationToken ct = default);
}
