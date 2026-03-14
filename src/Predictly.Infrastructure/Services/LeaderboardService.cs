using Microsoft.EntityFrameworkCore;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Infrastructure.Services;

public class LeaderboardService(PredictlyDbContext db) : ILeaderboardService
{
    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetTournamentLeaderboardAsync(
        Guid tournamentId, CancellationToken ct = default)
    {
        return await db.TournamentLeaderboard
            .Where(l => l.TournamentId == tournamentId)
            .OrderBy(l => l.Rank)
            .Join(db.Users, l => l.UserId, u => u.Id, (l, u) => new LeaderboardEntryDto(
                l.Rank,
                l.UserId,
                u.Username,
                l.TotalPoints,
                l.BonusParticipationCount,
                l.AvgFinalizedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(CancellationToken ct = default)
    {
        return await db.GlobalLeaderboard
            .OrderBy(l => l.Rank)
            .Join(db.Users, l => l.UserId, u => u.Id, (l, u) => new LeaderboardEntryDto(
                l.Rank,
                l.UserId,
                u.Username,
                l.TotalPoints,
                l.BonusParticipationCount,
                l.AvgFinalizedAt))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Full rebuild using raw SQL with window functions (spec §7).
    /// Ranking order: total_points desc, avg_finalized_at asc,
    ///                bonus_participation_count desc, user_id asc.
    /// </summary>
    public async Task RebuildAsync(Guid tournamentId, CancellationToken ct = default)
    {
        await RebuildTournamentLeaderboardAsync(tournamentId, ct);
        await RebuildGlobalLeaderboardAsync(ct);
    }

    private async Task RebuildTournamentLeaderboardAsync(Guid tournamentId, CancellationToken ct)
    {
        // Delete stale entries for this tournament then reinsert with fresh ranks
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM tournament_leaderboard WHERE tournament_id = {tournamentId}", ct);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO tournament_leaderboard
                (tournament_id, user_id, total_points, avg_finalized_at,
                 bonus_participation_count, rank, last_updated_at)
            SELECT
                m.tournament_id,
                ps.user_id,
                SUM(ps.total_score)                        AS total_points,
                AVG(p.finalized_at)                        AS avg_finalized_at,
                SUM(CASE WHEN ps.answered_any_bonus THEN 1 ELSE 0 END)
                                                           AS bonus_participation_count,
                rank() OVER (
                    ORDER BY
                        SUM(ps.total_score)                             DESC,
                        AVG(p.finalized_at)                             ASC,
                        SUM(CASE WHEN ps.answered_any_bonus THEN 1 ELSE 0 END) DESC,
                        ps.user_id                                      ASC
                )                                          AS rank,
                now()                                      AS last_updated_at
            FROM prediction_scores ps
            JOIN predictions       p  ON p.id         = ps.prediction_id
            JOIN matches           m  ON m.id         = ps.match_id
            WHERE m.tournament_id = {tournamentId}
            GROUP BY m.tournament_id, ps.user_id
            """, ct);
    }

    private async Task RebuildGlobalLeaderboardAsync(CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM global_leaderboard", ct);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO global_leaderboard
                (user_id, total_points, avg_finalized_at,
                 bonus_participation_count, rank, last_updated_at)
            SELECT
                ps.user_id,
                SUM(ps.total_score)                        AS total_points,
                AVG(p.finalized_at)                        AS avg_finalized_at,
                SUM(CASE WHEN ps.answered_any_bonus THEN 1 ELSE 0 END)
                                                           AS bonus_participation_count,
                rank() OVER (
                    ORDER BY
                        SUM(ps.total_score)                             DESC,
                        AVG(p.finalized_at)                             ASC,
                        SUM(CASE WHEN ps.answered_any_bonus THEN 1 ELSE 0 END) DESC,
                        ps.user_id                                      ASC
                )                                          AS rank,
                now()                                      AS last_updated_at
            FROM prediction_scores ps
            JOIN predictions p ON p.id = ps.prediction_id
            GROUP BY ps.user_id
            """, ct);
    }
}
