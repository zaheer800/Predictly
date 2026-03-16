namespace Predictly.Application.DTOs.Tournaments;

public record CreateTournamentRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyList<int> BonusQuestionIds);
