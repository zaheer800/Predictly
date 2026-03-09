using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Repositories.Interfaces;

public interface ILeaderboardRepository
{
    Task<IReadOnlyList<TournamentLeaderboard>> GetTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalLeaderboard>> GetGlobalLeaderboardAsync(CancellationToken ct = default);
    Task RebuildTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default);
    Task RebuildGlobalLeaderboardAsync(CancellationToken ct = default);
}
