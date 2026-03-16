using Predictly.Application.DTOs.Leaderboard;

namespace Predictly.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IReadOnlyList<TournamentLeaderboardEntry>> GetTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalLeaderboardEntry>> GetGlobalLeaderboardAsync(CancellationToken ct = default);
}
