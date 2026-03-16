namespace Predictly.Domain.Entities;

public class Prediction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public required string PredictedWinner { get; set; }
    public DateTime FinalizedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Match Match { get; set; } = null!;
    public ICollection<PredictionBonusAnswer> BonusAnswers { get; set; } = [];
    public PredictionScore? Score { get; set; }
}
