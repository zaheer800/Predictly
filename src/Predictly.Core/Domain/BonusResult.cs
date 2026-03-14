namespace Predictly.Core.Domain;

/// <summary>
/// Actual result for a bonus question after the match completes.
/// </summary>
public class BonusResult
{
    public BonusQuestionType QuestionType { get; init; }

    /// <summary>
    /// For Numeric questions: the actual numeric value.
    /// For MultipleChoice questions: the correct option (as a string).
    /// </summary>
    public string ActualValue { get; init; } = string.Empty;
}
