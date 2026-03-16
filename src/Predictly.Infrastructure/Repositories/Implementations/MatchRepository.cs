using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

public class MatchRepository : IMatchRepository
{
    private readonly PredictlyDbContext _db;

    public MatchRepository(PredictlyDbContext db) => _db = db;

    public Task<Match?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Matches.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<IReadOnlyList<Match>> GetByTournamentIdAsync(int tournamentId, CancellationToken ct = default) =>
        _db.Matches
           .Where(m => m.TournamentId == tournamentId)
           .OrderBy(m => m.MatchStartTime)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Match>)t.Result, ct);

    public async Task<Match> AddAsync(Match match, CancellationToken ct = default)
    {
        _db.Matches.Add(match);
        await _db.SaveChangesAsync(ct);
        return match;
    }

    public async Task UpdateAsync(Match match, CancellationToken ct = default)
    {
        _db.Matches.Update(match);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsByKeyAsync(int tournamentId, string teamHome, string teamAway, DateTime matchStartTime, CancellationToken ct = default) =>
        _db.Matches.AnyAsync(m =>
            m.TournamentId == tournamentId &&
            m.TeamHome == teamHome &&
            m.TeamAway == teamAway &&
            m.MatchStartTime == matchStartTime, ct);

    public Task<IReadOnlyList<Match>> GetScheduledWithSelectionsAsync(int tournamentId, CancellationToken ct = default) =>
        _db.Matches
           .Include(m => m.BonusSelections)
               .ThenInclude(s => s.BonusQuestion)
           .Where(m => m.TournamentId == tournamentId && m.Status == MatchStatus.Scheduled)
           .OrderBy(m => m.MatchStartTime)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Match>)t.Result, ct);

    public Task<Match?> GetForScoringAsync(int id, CancellationToken ct = default) =>
        _db.Matches
           .Include(m => m.BonusSelections)
               .ThenInclude(s => s.BonusQuestion)
           .Include(m => m.BonusResults)
           .FirstOrDefaultAsync(m => m.Id == id, ct);
}
