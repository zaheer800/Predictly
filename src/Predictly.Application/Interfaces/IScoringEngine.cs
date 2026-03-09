using Predictly.Domain.Entities;

namespace Predictly.Application.Interfaces;

/// <summary>
/// Pure, side-effect-free scoring calculations.
/// All methods are deterministic given the same inputs.
/// </summary>
public interface IScoringEngine
{
    int CalculateWinnerScore(string predictedWinner, string actualWinner);
    int CalculateNumericBonusScore(decimal predicted, decimal actual, int maxPoints);
    int CalculateMultipleChoiceScore(string predictedChoice, string actualChoice, int maxPoints);
    PredictionScore ComputePredictionScore(
        Prediction prediction,
        string actualWinner,
        IReadOnlyList<MatchBonusSelection> bonusSelections,
        IReadOnlyList<MatchBonusResult> bonusResults,
        int tournamentId);
}
