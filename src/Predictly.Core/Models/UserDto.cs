namespace Predictly.Core.Models;

public record UserDto(Guid Id, string Username, string Email, string Role, DateTimeOffset CreatedAt);
