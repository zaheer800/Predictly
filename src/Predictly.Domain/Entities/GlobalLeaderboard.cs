namespace Predictly.Domain.Entities;

public class GlobalLeaderboard
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TotalPoints { get; set; }
    public DateTime AvgFinalizedAt { get; set; }
    public int BonusParticipationCount { get; set; }
    public int Rank { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
