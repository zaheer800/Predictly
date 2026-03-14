namespace Predictly.Infrastructure.Entities;

/// <summary>
/// The 3 bonus questions randomly selected for a specific match.
/// </summary>
public class MatchBonusSelection
{
    public Guid MatchId { get; set; }
    public int BonusQuestionCatalogId { get; set; }
    public int DisplayOrder { get; set; } // 1–3

    public Match Match { get; set; } = null!;
    public BonusQuestionCatalog BonusQuestion { get; set; } = null!;
    public ICollection<MatchBonusResult> Results { get; set; } = [];
}
