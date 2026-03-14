namespace Predictly.Infrastructure.Entities;

/// <summary>
/// Admin-submitted actual result for a bonus question in a match.
/// </summary>
public class MatchBonusResult
{
    public Guid MatchId { get; set; }
    public int BonusQuestionCatalogId { get; set; }
    public string ActualValue { get; set; } = string.Empty;

    public MatchBonusSelection Selection { get; set; } = null!;
}
