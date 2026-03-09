namespace Predictly.Application.Interfaces;

/// <summary>
/// Orchestrates the full scoring flow for a completed match.
/// Steps: validate → score all predictions → persist → rebuild leaderboards.
/// </summary>
public interface IMatchScoringService
{
    Task ScoreMatchAsync(int matchId, CancellationToken ct = default);
}
