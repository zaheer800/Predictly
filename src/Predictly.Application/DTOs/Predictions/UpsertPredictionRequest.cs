namespace Predictly.Application.DTOs.Predictions;

public record BonusAnswerInput(int MatchBonusSelectionId, decimal? AnswerNumeric, string? AnswerChoice);

public record UpsertPredictionRequest(
    int MatchId,
    string PredictedWinner,
    IReadOnlyList<BonusAnswerInput>? BonusAnswers);
