namespace Predictly.Application.DTOs.Predictions;

public record BonusAnswerResponse(int MatchBonusSelectionId, decimal? AnswerNumeric, string? AnswerChoice);

public record PredictionResponse(
    int Id,
    int UserId,
    int MatchId,
    string PredictedWinner,
    DateTime FinalizedAt,
    IReadOnlyList<BonusAnswerResponse> BonusAnswers);
