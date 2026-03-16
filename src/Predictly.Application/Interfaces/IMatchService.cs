using Predictly.Application.DTOs.Matches;

namespace Predictly.Application.Interfaces;

public interface IMatchService
{
    Task<MatchResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MatchResponse>> GetByTournamentAsync(int tournamentId, CancellationToken ct = default);
}
