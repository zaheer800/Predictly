namespace Predictly.Core.Models;

public record MatchDto(
    Guid Id,
    Guid TournamentId,
    string TeamA,
    string TeamB,
    DateTimeOffset MatchStartTime,
    string Status,
    string? WinnerTeam);

public record CreateMatchRequest(
    Guid TournamentId,
    string TeamA,
    string TeamB,
    DateTimeOffset MatchStartTime);

public record CompleteMatchRequest(
    string WinnerTeam,
    IReadOnlyList<BonusResultRequest> BonusResults);

public record BonusResultRequest(int BonusQuestionCatalogId, string ActualValue);
