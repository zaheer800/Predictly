using Microsoft.EntityFrameworkCore;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;
using InvalidOperationException = Predictly.Core.Exceptions.InvalidOperationException;

namespace Predictly.Infrastructure.Services;

public class TournamentService(PredictlyDbContext db) : ITournamentService
{
    public async Task<TournamentDto> CreateAsync(CreateTournamentRequest request, CancellationToken ct = default)
    {
        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Status = "Draft",
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync(ct);
        return ToDto(tournament);
    }

    public async Task<TournamentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await db.Tournaments.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Tournament), id);
        return ToDto(tournament);
    }

    public async Task<IReadOnlyList<TournamentDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await db.Tournaments.OrderBy(t => t.CreatedAt).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await db.Tournaments.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Tournament), id);

        if (tournament.Status != "Draft")
            throw new InvalidOperationException($"Tournament '{id}' cannot be activated from status '{tournament.Status}'.");

        tournament.Status = "Active";
        await db.SaveChangesAsync(ct);
    }

    private static TournamentDto ToDto(Tournament t) => new(t.Id, t.Name, t.Status, t.CreatedAt);
}
