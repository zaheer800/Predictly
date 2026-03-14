namespace Predictly.Core.Models;

public record TournamentDto(Guid Id, string Name, string Status, DateTimeOffset CreatedAt);

public record CreateTournamentRequest(string Name);
