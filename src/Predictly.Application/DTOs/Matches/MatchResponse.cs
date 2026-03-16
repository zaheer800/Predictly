namespace Predictly.Application.DTOs.Matches;

public record BonusQuestionSlot(int SelectionId, string QuestionKey, string QuestionTemplate, string AnswerType, string[]? Options, int MaxPoints);

public record MatchResponse(
    int Id,
    int TournamentId,
    string TeamHome,
    string TeamAway,
    string? Venue,
    DateTime MatchStartTime,
    string Status,
    string? Winner,
    IReadOnlyList<BonusQuestionSlot> BonusQuestions);
