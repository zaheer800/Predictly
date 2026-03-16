using Predictly.Domain.Enums;

namespace Predictly.Domain.Entities;

public class BonusQuestionCatalog
{
    public int Id { get; set; }
    public required string QuestionKey { get; set; }
    public required string QuestionTemplate { get; set; }
    public AnswerType AnswerType { get; set; }
    public string[]? Options { get; set; }  // JSONB — null for numeric types
    public int MaxPoints { get; set; }
    public ScoringRule ScoringRule { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<TournamentBonusConfig> TournamentConfigs { get; set; } = [];
    public ICollection<MatchBonusSelection> MatchSelections { get; set; } = [];
}
