namespace Predictly.Core.Interfaces;

public interface IScoringService
{
    /// <summary>
    /// Scores all predictions for a completed match and persists prediction_scores.
    /// Must be called within a transaction.
    /// </summary>
    Task ScoreMatchAsync(Guid matchId, CancellationToken ct = default);
}
