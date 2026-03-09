using Predictly.Application.Interfaces;
using Predictly.Domain.Constants;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Application.Services;

/// <summary>
/// Acting as the Developer (BMAD): Implements all scoring formulas exactly as specified in
/// docs/02_bonus_question_catalog.md and CLAUDE.md §8.3.
///
/// CRITICAL RULES:
/// - Pure functions — no I/O, no side effects, no randomness.
/// - All formulas are deterministic given the same inputs.
/// - floor(max(0, ...)) applied to all numeric scores.
/// - actual == 0 special case: predicted == 0 → full points, else → 0.
/// </summary>
public class ScoringEngine : IScoringEngine
{
    /// <summary>
    /// Awards WinnerPoints if predictedWinner exactly matches actualWinner, else 0.
    /// String comparison is case-sensitive (teams are stored as entered).
    /// </summary>
    public int CalculateWinnerScore(string predictedWinner, string actualWinner)
    {
        if (string.IsNullOrWhiteSpace(predictedWinner) || string.IsNullOrWhiteSpace(actualWinner))
            return 0;

        return predictedWinner == actualWinner ? ScoringConstants.WinnerPoints : 0;
    }

    /// <summary>
    /// Percentage-based partial credit for numeric bonus questions.
    ///
    /// Formula: floor(max(0, maxPoints × (1 − |predicted − actual| / actual)))
    /// Special case: if actual == 0 → predicted == 0 gives full points, else 0.
    /// </summary>
    public int CalculateNumericBonusScore(decimal predicted, decimal actual, int maxPoints)
    {
        if (actual == 0)
            return predicted == 0 ? maxPoints : 0;

        double percentageDiff = (double)Math.Abs(predicted - actual) / (double)actual;
        double rawScore = maxPoints * (1.0 - percentageDiff);
        return (int)Math.Floor(Math.Max(0.0, rawScore));
    }

    /// <summary>
    /// All-or-nothing scoring for multiple choice bonus questions.
    /// </summary>
    public int CalculateMultipleChoiceScore(string predictedChoice, string actualChoice, int maxPoints)
    {
        if (string.IsNullOrWhiteSpace(predictedChoice) || string.IsNullOrWhiteSpace(actualChoice))
            return 0;

        return predictedChoice == actualChoice ? maxPoints : 0;
    }

    /// <summary>
    /// Computes a full PredictionScore for one prediction against the match's actual results.
    /// BonusSelections are ordered by display_order (1, 2, 3) — scores map to bonus_score_1/2/3.
    /// </summary>
    public PredictionScore ComputePredictionScore(
        Prediction prediction,
        string actualWinner,
        IReadOnlyList<MatchBonusSelection> bonusSelections,
        IReadOnlyList<MatchBonusResult> bonusResults,
        int tournamentId)
    {
        var winnerScore = CalculateWinnerScore(prediction.PredictedWinner, actualWinner);

        var orderedSelections = bonusSelections.OrderBy(s => s.DisplayOrder).ToList();
        var bonusScores = new int[ScoringConstants.MaxBonusQuestionsPerMatch];
        short answeredCount = 0;

        for (int i = 0; i < orderedSelections.Count; i++)
        {
            var selection = orderedSelections[i];
            var result = bonusResults.FirstOrDefault(r => r.MatchBonusSelectionId == selection.Id);
            var answer = prediction.BonusAnswers.FirstOrDefault(a => a.MatchBonusSelectionId == selection.Id);

            if (result is null || answer is null)
                continue;

            answeredCount++;
            bonusScores[i] = ComputeSingleBonusScore(answer, result, selection.BonusQuestion);
        }

        return new PredictionScore
        {
            PredictionId        = prediction.Id,
            UserId              = prediction.UserId,
            MatchId             = prediction.MatchId,
            TournamentId        = tournamentId,
            WinnerScore         = winnerScore,
            BonusScore1         = bonusScores[0],
            BonusScore2         = bonusScores[1],
            BonusScore3         = bonusScores[2],
            BonusAnsweredCount  = answeredCount,
            ScoredAt            = DateTime.UtcNow
        };
    }

    private int ComputeSingleBonusScore(
        PredictionBonusAnswer answer,
        MatchBonusResult result,
        BonusQuestionCatalog question)
    {
        return question.ScoringRule switch
        {
            ScoringRule.PercentageBased => answer.AnswerNumeric.HasValue && result.ActualNumeric.HasValue
                ? CalculateNumericBonusScore(answer.AnswerNumeric.Value, result.ActualNumeric.Value, question.MaxPoints)
                : 0,

            ScoringRule.ExactMatch => !string.IsNullOrWhiteSpace(answer.AnswerChoice) && !string.IsNullOrWhiteSpace(result.ActualChoice)
                ? CalculateMultipleChoiceScore(answer.AnswerChoice, result.ActualChoice, question.MaxPoints)
                : 0,

            _ => 0
        };
    }
}
