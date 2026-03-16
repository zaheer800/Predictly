namespace Predictly.Domain.Entities;

public class TournamentLeaderboard
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int UserId { get; set; }
    public int TotalPoints { get; set; }
    public DateTime AvgFinalizedAt { get; set; }
    public int BonusParticipationCount { get; set; }
    public int Rank { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = null!;
    public User User { get; set; } = null!;
}
