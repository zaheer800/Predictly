namespace Predictly.Infrastructure.Entities;

public class Prediction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }
    public string PredictedWinner { get; set; } = string.Empty;
    public DateTimeOffset FinalizedAt { get; set; } // UTC — updated on each edit

    public User User { get; set; } = null!;
    public Match Match { get; set; } = null!;
    public ICollection<PredictionBonusAnswer> BonusAnswers { get; set; } = [];
    public PredictionScore? Score { get; set; }
}
