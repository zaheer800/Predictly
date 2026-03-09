using Predictly.Domain.Enums;

namespace Predictly.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public required string TeamHome { get; set; }
    public required string TeamAway { get; set; }
    public string? Venue { get; set; }
    public DateTime MatchStartTime { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public string? Winner { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<MatchBonusSelection> BonusSelections { get; set; } = [];
    public ICollection<Prediction> Predictions { get; set; } = [];
    public ICollection<MatchBonusResult> BonusResults { get; set; } = [];
}
