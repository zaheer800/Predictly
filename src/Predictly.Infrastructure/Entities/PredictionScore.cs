namespace Predictly.Infrastructure.Entities;

public class PredictionScore
{
    public Guid PredictionId { get; set; }
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public int WinnerScore { get; set; }
    public int BonusScore { get; set; }
    public int TotalScore { get; set; }
    public bool AnsweredAnyBonus { get; set; }

    public Prediction Prediction { get; set; } = null!;
    public Match Match { get; set; } = null!;
}
