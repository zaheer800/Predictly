namespace Predictly.Core.Models;

public record BonusQuestionDto(
    int Id,
    string QuestionText,
    string QuestionType,
    string? Options,
    int MaxPoints);

public record SetBonusConfigRequest(IReadOnlyList<int> BonusQuestionCatalogIds);
