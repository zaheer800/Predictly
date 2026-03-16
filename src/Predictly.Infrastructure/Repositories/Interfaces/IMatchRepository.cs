using Predictly.Domain.Entities;

namespace Predictly.Infrastructure.Repositories.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Match>> GetByTournamentIdAsync(int tournamentId, CancellationToken ct = default);
    Task<Match> AddAsync(Match match, CancellationToken ct = default);
    Task UpdateAsync(Match match, CancellationToken ct = default);
    Task<bool> ExistsByKeyAsync(int tournamentId, string teamHome, string teamAway, DateTime matchStartTime, CancellationToken ct = default);
    Task<IReadOnlyList<Match>> GetScheduledWithSelectionsAsync(int tournamentId, CancellationToken ct = default);
    Task<Match?> GetForScoringAsync(int id, CancellationToken ct = default);
}
