namespace Predictly.Application.DTOs.Admin;

public record UploadSummaryResponse(
    int MatchesCreated,
    int MatchesUpdated,
    int ResultsProcessed,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors);
