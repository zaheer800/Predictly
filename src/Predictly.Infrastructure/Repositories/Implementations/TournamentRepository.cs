using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

public class TournamentRepository : ITournamentRepository
{
    private readonly PredictlyDbContext _db;

    public TournamentRepository(PredictlyDbContext db) => _db = db;

    public Task<Tournament?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Tournaments
           .Include(t => t.BonusConfig)
               .ThenInclude(bc => bc.BonusQuestion)
           .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<IReadOnlyList<Tournament>> GetAllAsync(CancellationToken ct = default) =>
        _db.Tournaments
           .OrderByDescending(t => t.CreatedAt)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Tournament>)t.Result, ct);

    public Task<IReadOnlyList<Tournament>> GetByStatusAsync(TournamentStatus status, CancellationToken ct = default) =>
        _db.Tournaments
           .Where(t => t.Status == status)
           .OrderByDescending(t => t.CreatedAt)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Tournament>)t.Result, ct);

    public async Task<Tournament> AddAsync(Tournament tournament, CancellationToken ct = default)
    {
        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync(ct);
        return tournament;
    }

    public async Task UpdateAsync(Tournament tournament, CancellationToken ct = default)
    {
        _db.Tournaments.Update(tournament);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> HasActiveBonusConfigAsync(int tournamentId, CancellationToken ct = default) =>
        _db.TournamentBonusConfigs.AnyAsync(bc => bc.TournamentId == tournamentId, ct);
}
