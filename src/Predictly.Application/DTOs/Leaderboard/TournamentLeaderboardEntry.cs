namespace Predictly.Application.DTOs.Leaderboard;

public record TournamentLeaderboardEntry(
    int Rank,
    int UserId,
    string DisplayName,
    int TotalPoints,
    int BonusParticipationCount);
