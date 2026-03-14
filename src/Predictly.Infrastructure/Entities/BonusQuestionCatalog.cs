namespace Predictly.Infrastructure.Entities;

/// <summary>
/// System-level catalog of all available bonus question types (10 predefined entries).
/// </summary>
public class BonusQuestionCatalog
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty; // "Numeric" | "MultipleChoice"
    public string? Options { get; set; } // JSON array for MultipleChoice, null for Numeric
    public int MaxPoints { get; set; }

    public ICollection<TournamentBonusConfig> TournamentConfigs { get; set; } = [];
    public ICollection<MatchBonusSelection> MatchSelections { get; set; } = [];
}
