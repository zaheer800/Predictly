using Predictly.Application.DTOs.Leaderboard;
using Predictly.Application.Interfaces;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _leaderboardRepo;

    public LeaderboardService(ILeaderboardRepository leaderboardRepo) =>
        _leaderboardRepo = leaderboardRepo;

    public async Task<IReadOnlyList<TournamentLeaderboardEntry>> GetTournamentLeaderboardAsync(
        int tournamentId, CancellationToken ct = default)
    {
        var entries = await _leaderboardRepo.GetTournamentLeaderboardAsync(tournamentId, ct);
        return entries
            .Select(e => new TournamentLeaderboardEntry(
                e.Rank,
                e.UserId,
                e.User.DisplayName,
                e.TotalPoints,
                e.BonusParticipationCount))
            .ToList();
    }

    public async Task<IReadOnlyList<GlobalLeaderboardEntry>> GetGlobalLeaderboardAsync(
        CancellationToken ct = default)
    {
        var entries = await _leaderboardRepo.GetGlobalLeaderboardAsync(ct);
        return entries
            .Select(e => new GlobalLeaderboardEntry(
                e.Rank,
                e.UserId,
                e.User.DisplayName,
                e.TotalPoints,
                e.BonusParticipationCount))
            .ToList();
    }
}
