namespace Predictly.Core.Models;

public record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string Username,
    int TotalPoints,
    int BonusParticipationCount,
    DateTimeOffset AvgFinalizedAt);
