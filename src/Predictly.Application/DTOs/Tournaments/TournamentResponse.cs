namespace Predictly.Application.DTOs.Tournaments;

public record TournamentResponse(
    int Id,
    string Name,
    string? Description,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<string> AllowedBonusQuestionKeys);
