using Predictly.Domain.Enums;

namespace Predictly.Domain.Entities;

public class Tournament
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User Creator { get; set; } = null!;
    public ICollection<Match> Matches { get; set; } = [];
    public ICollection<TournamentBonusConfig> BonusConfig { get; set; } = [];
    public ICollection<TournamentLeaderboard> Leaderboard { get; set; } = [];
}
