namespace Predictly.Core.Models;

public record PredictionDto(
    Guid Id,
    Guid UserId,
    Guid MatchId,
    string PredictedWinner,
    IReadOnlyList<BonusAnswerDto> BonusAnswers,
    DateTimeOffset FinalizedAt);

public record BonusAnswerDto(int BonusQuestionCatalogId, string AnswerValue);

public record UpsertPredictionRequest(
    Guid MatchId,
    string PredictedWinner,
    IReadOnlyList<BonusAnswerDto> BonusAnswers);
