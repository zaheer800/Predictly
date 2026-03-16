using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

/// <summary>
/// Leaderboard reads and full rebuilds using raw SQL with rank() window function.
/// Per architecture: raw SQL is permitted here for aggregation queries.
/// </summary>
public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly PredictlyDbContext _db;

    public LeaderboardRepository(PredictlyDbContext db) => _db = db;

    public Task<IReadOnlyList<TournamentLeaderboard>> GetTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default) =>
        _db.TournamentLeaderboard
           .Where(t => t.TournamentId == tournamentId)
           .OrderBy(t => t.Rank)
           .Include(t => t.User)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<TournamentLeaderboard>)t.Result, ct);

    public Task<IReadOnlyList<GlobalLeaderboard>> GetGlobalLeaderboardAsync(CancellationToken ct = default) =>
        _db.GlobalLeaderboard
           .OrderBy(g => g.Rank)
           .Include(g => g.User)
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<GlobalLeaderboard>)t.Result, ct);

    public async Task RebuildTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM tournament_leaderboard WHERE tournament_id = {tournamentId}", ct);

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO tournament_leaderboard
                (tournament_id, user_id, total_points, avg_finalized_at,
                 bonus_participation_count, rank, last_updated_at)
            SELECT
                ps.tournament_id,
                ps.user_id,
                SUM(ps.total_score)                              AS total_points,
                AVG(p.finalized_at)                              AS avg_finalized_at,
                SUM(ps.bonus_answered_count)                     AS bonus_participation_count,
                RANK() OVER (
                    ORDER BY
                        SUM(ps.total_score)          DESC,
                        AVG(p.finalized_at)          ASC,
                        SUM(ps.bonus_answered_count) DESC,
                        ps.user_id                   ASC
                )                                                AS rank,
                now()
            FROM prediction_scores ps
            JOIN predictions p ON p.id = ps.prediction_id
            WHERE ps.tournament_id = {tournamentId}
            GROUP BY ps.tournament_id, ps.user_id
            """, ct);
    }

    public async Task RebuildGlobalLeaderboardAsync(CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlRawAsync("TRUNCATE global_leaderboard", ct);

        await _db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO global_leaderboard
                (user_id, total_points, avg_finalized_at,
                 bonus_participation_count, rank, last_updated_at)
            SELECT
                ps.user_id,
                SUM(ps.total_score)                              AS total_points,
                AVG(p.finalized_at)                              AS avg_finalized_at,
                SUM(ps.bonus_answered_count)                     AS bonus_participation_count,
                RANK() OVER (
                    ORDER BY
                        SUM(ps.total_score)          DESC,
                        AVG(p.finalized_at)          ASC,
                        SUM(ps.bonus_answered_count) DESC,
                        ps.user_id                   ASC
                )                                                AS rank,
                now()
            FROM prediction_scores ps
            JOIN predictions p ON p.id = ps.prediction_id
            GROUP BY ps.user_id
            """, ct);
    }
}
