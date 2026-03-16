using Predictly.Application.DTOs.Predictions;

namespace Predictly.Application.Interfaces;

public interface IPredictionService
{
    Task<PredictionResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PredictionResponse?> GetByUserAndMatchAsync(int userId, int matchId, CancellationToken ct = default);
    Task<PredictionResponse> UpsertAsync(UpsertPredictionRequest request, int userId, CancellationToken ct = default);
}
