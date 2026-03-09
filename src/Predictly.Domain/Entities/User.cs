using Predictly.Domain.Enums;

namespace Predictly.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Tournament> CreatedTournaments { get; set; } = [];
    public ICollection<Match> CreatedMatches { get; set; } = [];
    public ICollection<Prediction> Predictions { get; set; } = [];
    public TournamentLeaderboard? TournamentLeaderboard { get; set; }
    public GlobalLeaderboard? GlobalLeaderboard { get; set; }
}
