using Predictly.Core.Domain;

namespace Predictly.Core.Scoring;

/// <summary>
/// Deterministic scoring engine for Predictly match predictions.
///
/// Scoring rules (from system design §2.3):
///
/// Winner points:
///   Exact team match → fixed WinnerPoints constant; otherwise 0.
///
/// Numeric bonus formula:
///   percentage_difference = abs(predicted - actual) / actual
///   raw_score             = max_points * (1 - percentage_difference)
///   score                 = max(0, raw_score)
///   final_score           = floor(score)
///
///   Special case when actual == 0:
///     predicted == 0 → full points
///     otherwise      → 0
///
/// Multiple-choice bonus:
///   Exact string match → full points; otherwise 0.
/// </summary>
public static class ScoringEngine
{
    /// <summary>
    /// Scores all predictions for a completed match.
    /// </summary>
    public static IReadOnlyList<PredictionScore> ScoreMatch(
        IEnumerable<PredictionInput> predictions,
        IReadOnlyList<BonusResult> bonusResults)
    {
        return predictions
            .Select(p => ScorePrediction(p, bonusResults))
            .ToList();
    }

    /// <summary>
    /// Scores a single user's prediction.
    /// </summary>
    public static PredictionScore ScorePrediction(
        PredictionInput prediction,
        IReadOnlyList<BonusResult> bonusResults)
    {
        int winnerScore = ScoreWinner(prediction.PredictedWinner, prediction.ActualWinner, prediction.WinnerPoints);

        var bonusScores = ScoreBonusAnswers(prediction.BonusAnswers, bonusResults);

        return new PredictionScore
        {
            UserId = prediction.UserId,
            WinnerScore = winnerScore,
            BonusScores = bonusScores
        };
    }

    /// <summary>
    /// Awards WinnerPoints for an exact team name match, 0 otherwise.
    /// </summary>
    public static int ScoreWinner(string predictedWinner, string actualWinner, int winnerPoints)
    {
        return string.Equals(predictedWinner, actualWinner, StringComparison.OrdinalIgnoreCase)
            ? winnerPoints
            : 0;
    }

    /// <summary>
    /// Scores each bonus answer against its corresponding result.
    /// Answers without a matching result (index out of range) are ignored.
    /// </summary>
    public static IReadOnlyList<int> ScoreBonusAnswers(
        IReadOnlyList<BonusAnswer> answers,
        IReadOnlyList<BonusResult> results)
    {
        var scores = new List<int>(answers.Count);

        for (int i = 0; i < answers.Count && i < results.Count; i++)
        {
            scores.Add(ScoreBonusAnswer(answers[i], results[i]));
        }

        return scores;
    }

    /// <summary>
    /// Scores a single bonus answer against its result.
    /// </summary>
    public static int ScoreBonusAnswer(BonusAnswer answer, BonusResult result)
    {
        return result.QuestionType switch
        {
            BonusQuestionType.Numeric => ScoreNumericBonus(answer.PredictedValue, result.ActualValue, answer.MaxPoints),
            BonusQuestionType.MultipleChoice => ScoreMultipleChoiceBonus(answer.PredictedValue, result.ActualValue, answer.MaxPoints),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.QuestionType, "Unknown bonus question type.")
        };
    }

    /// <summary>
    /// Numeric bonus scoring formula from system design §2.3:
    ///   percentage_difference = abs(predicted - actual) / actual
    ///   raw_score             = max_points * (1 - percentage_difference)
    ///   score                 = max(0, raw_score)
    ///   final_score           = floor(score)
    ///
    /// Special case: actual == 0 → predicted == 0 gives full points, otherwise 0.
    /// </summary>
    public static int ScoreNumericBonus(string predictedStr, string actualStr, int maxPoints)
    {
        if (!decimal.TryParse(predictedStr, out decimal predicted) ||
            !decimal.TryParse(actualStr, out decimal actual))
        {
            return 0;
        }

        if (actual == 0)
        {
            return predicted == 0 ? maxPoints : 0;
        }

        decimal percentageDiff = Math.Abs(predicted - actual) / actual;
        decimal rawScore = maxPoints * (1 - percentageDiff);
        decimal score = Math.Max(0, rawScore);
        return (int)Math.Floor(score);
    }

    /// <summary>
    /// Multiple-choice bonus: exact case-insensitive match → full points, otherwise 0.
    /// </summary>
    public static int ScoreMultipleChoiceBonus(string predicted, string actual, int maxPoints)
    {
        return string.Equals(predicted, actual, StringComparison.OrdinalIgnoreCase)
            ? maxPoints
            : 0;
    }
}
