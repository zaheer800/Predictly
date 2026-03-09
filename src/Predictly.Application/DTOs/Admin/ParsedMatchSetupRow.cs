namespace Predictly.Application.DTOs.Admin;

public record ParsedMatchSetupRow(
    int TournamentId,
    string TeamHome,
    string TeamAway,
    string? Venue,
    DateTime MatchStartTime);
