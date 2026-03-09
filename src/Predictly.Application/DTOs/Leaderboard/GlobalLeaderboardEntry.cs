namespace Predictly.Application.DTOs.Leaderboard;

public record GlobalLeaderboardEntry(
    int Rank,
    int UserId,
    string DisplayName,
    int TotalPoints,
    int BonusParticipationCount);
