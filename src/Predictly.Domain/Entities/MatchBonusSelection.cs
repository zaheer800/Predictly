namespace Predictly.Domain.Entities;

public class MatchBonusSelection
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int BonusQuestionId { get; set; }
    public short DisplayOrder { get; set; }  // 1, 2, or 3
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
    public BonusQuestionCatalog BonusQuestion { get; set; } = null!;
    public ICollection<PredictionBonusAnswer> PredictionAnswers { get; set; } = [];
    public MatchBonusResult? BonusResult { get; set; }
}
