namespace Predictly.Application.DTOs.Auth;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string DisplayName);
