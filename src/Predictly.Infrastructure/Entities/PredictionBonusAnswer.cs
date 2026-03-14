namespace Predictly.Infrastructure.Entities;

public class PredictionBonusAnswer
{
    public Guid PredictionId { get; set; }
    public int BonusQuestionCatalogId { get; set; }
    public string AnswerValue { get; set; } = string.Empty;

    public Prediction Prediction { get; set; } = null!;
    public BonusQuestionCatalog BonusQuestion { get; set; } = null!;
}
