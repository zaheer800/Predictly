using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface IMatchService
{
    Task<MatchDto> CreateAsync(CreateMatchRequest request, CancellationToken ct = default);
    Task<MatchDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MatchDto>> ListByTournamentAsync(Guid tournamentId, CancellationToken ct = default);

    /// <summary>
    /// Admin submits match result, triggers scoring and leaderboard rebuild.
    /// </summary>
    Task CompleteAsync(Guid matchId, CompleteMatchRequest request, CancellationToken ct = default);
}
