namespace Predictly.Application.DTOs.Admin;

public record ParsedUpload(
    IReadOnlyList<ParsedMatchSetupRow> MatchSetups,
    IReadOnlyList<ParsedMatchResultRow> MatchResults,
    IReadOnlyList<string> ParseErrors);
