using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardEntryDto>> GetTournamentLeaderboardAsync(Guid tournamentId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(CancellationToken ct = default);

    /// <summary>
    /// Full rebuild of both tournament and global leaderboards.
    /// Called inside the match-completion transaction.
    /// </summary>
    Task RebuildAsync(Guid tournamentId, CancellationToken ct = default);
}
