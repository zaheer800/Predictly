namespace Predictly.Infrastructure.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public string TeamA { get; set; } = string.Empty;
    public string TeamB { get; set; } = string.Empty;
    public DateTimeOffset MatchStartTime { get; set; } // UTC — prediction lock point
    public string Status { get; set; } = "Scheduled"; // "Scheduled" | "Completed"
    public string? WinnerTeam { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public ICollection<MatchBonusSelection> BonusSelections { get; set; } = [];
    public ICollection<Prediction> Predictions { get; set; } = [];
    public ICollection<PredictionScore> PredictionScores { get; set; } = [];
}
