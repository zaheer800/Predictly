namespace Predictly.Core.Domain;

/// <summary>
/// Represents a user's answer to a bonus question for a specific match.
/// </summary>
public class BonusAnswer
{
    public BonusQuestionType QuestionType { get; init; }

    /// <summary>
    /// For Numeric questions: the predicted numeric value.
    /// For MultipleChoice questions: the predicted option (as a string).
    /// </summary>
    public string PredictedValue { get; init; } = string.Empty;

    public int MaxPoints { get; init; }
}
