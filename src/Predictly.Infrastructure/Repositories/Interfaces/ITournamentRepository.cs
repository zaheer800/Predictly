using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Repositories.Interfaces;

public interface ITournamentRepository
{
    Task<Tournament?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Tournament>> GetByStatusAsync(TournamentStatus status, CancellationToken ct = default);
    Task<Tournament> AddAsync(Tournament tournament, CancellationToken ct = default);
    Task UpdateAsync(Tournament tournament, CancellationToken ct = default);
    Task<bool> HasActiveBonusConfigAsync(int tournamentId, CancellationToken ct = default);
}
