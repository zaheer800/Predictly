namespace Predictly.Infrastructure.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "User" | "Admin"
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Prediction> Predictions { get; set; } = [];
    public ICollection<TournamentLeaderboard> TournamentLeaderboardEntries { get; set; } = [];
    public ICollection<GlobalLeaderboard> GlobalLeaderboardEntries { get; set; } = [];
}
