namespace Predictly.Domain.Entities;

public class PredictionBonusAnswer
{
    public int Id { get; set; }
    public int PredictionId { get; set; }
    public int MatchBonusSelectionId { get; set; }
    public decimal? AnswerNumeric { get; set; }
    public string? AnswerChoice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Prediction Prediction { get; set; } = null!;
    public MatchBonusSelection MatchBonusSelection { get; set; } = null!;
}
