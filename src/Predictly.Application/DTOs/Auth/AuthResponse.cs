namespace Predictly.Application.DTOs.Auth;

public record AuthResponse(
    int UserId,
    string Username,
    string DisplayName,
    string Role,
    string Token);
