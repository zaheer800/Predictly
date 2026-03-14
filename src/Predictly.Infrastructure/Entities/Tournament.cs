namespace Predictly.Infrastructure.Entities;

public class Tournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // "Draft" | "Active" | "Completed"
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Match> Matches { get; set; } = [];
    public ICollection<TournamentBonusConfig> BonusConfigs { get; set; } = [];
    public ICollection<TournamentLeaderboard> Leaderboard { get; set; } = [];
}
