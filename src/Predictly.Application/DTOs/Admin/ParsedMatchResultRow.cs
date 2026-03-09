namespace Predictly.Application.DTOs.Admin;

public record ParsedBonusResult(int SelectionId, decimal? ActualNumeric, string? ActualChoice);

public record ParsedMatchResultRow(
    int MatchId,
    string Winner,
    IReadOnlyList<ParsedBonusResult> BonusResults);
