namespace Predictly.Infrastructure.Entities;

/// <summary>
/// Precomputed leaderboard entry per user per tournament.
/// Fully rebuilt after each completed match.
/// </summary>
public class TournamentLeaderboard
{
    public Guid TournamentId { get; set; }
    public Guid UserId { get; set; }
    public int TotalPoints { get; set; }
    public DateTimeOffset AvgFinalizedAt { get; set; }
    public int BonusParticipationCount { get; set; }
    public int Rank { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public User User { get; set; } = null!;
}
