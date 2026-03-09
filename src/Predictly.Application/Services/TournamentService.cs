using Predictly.Application.DTOs.Tournaments;
using Predictly.Application.Interfaces;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;
using Predictly.Domain.Exceptions;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class TournamentService : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepo;

    public TournamentService(ITournamentRepository tournamentRepo) =>
        _tournamentRepo = tournamentRepo;

    public async Task<TournamentResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tournament), id);
        return MapToResponse(tournament);
    }

    public async Task<IReadOnlyList<TournamentResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tournaments = await _tournamentRepo.GetAllAsync(ct);
        return tournaments.Select(MapToResponse).ToList();
    }

    public async Task<TournamentResponse> CreateAsync(
        CreateTournamentRequest request, int createdBy, CancellationToken ct = default)
    {
        var tournament = new Tournament
        {
            Name        = request.Name,
            Description = request.Description,
            StartDate   = request.StartDate,
            EndDate     = request.EndDate,
            Status      = TournamentStatus.Draft,
            CreatedBy   = createdBy,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow,
            BonusConfig = request.BonusQuestionIds
                .Select(qId => new TournamentBonusConfig
                {
                    BonusQuestionId = qId,
                    CreatedAt       = DateTime.UtcNow
                }).ToList()
        };

        await _tournamentRepo.AddAsync(tournament, ct);
        return await GetByIdAsync(tournament.Id, ct);
    }

    public async Task<TournamentResponse> ActivateAsync(int id, CancellationToken ct = default)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tournament), id);

        if (tournament.Status != TournamentStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only Draft tournaments can be activated.");

        tournament.Status    = TournamentStatus.Active;
        tournament.UpdatedAt = DateTime.UtcNow;
        await _tournamentRepo.UpdateAsync(tournament, ct);
        return MapToResponse(tournament);
    }

    public async Task<TournamentResponse> CompleteAsync(int id, CancellationToken ct = default)
    {
        var tournament = await _tournamentRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tournament), id);

        if (tournament.Status != TournamentStatus.Active)
            throw new BusinessRuleException("INVALID_STATUS", "Only Active tournaments can be completed.");

        tournament.Status    = TournamentStatus.Completed;
        tournament.UpdatedAt = DateTime.UtcNow;
        await _tournamentRepo.UpdateAsync(tournament, ct);
        return MapToResponse(tournament);
    }

    private static TournamentResponse MapToResponse(Tournament t) =>
        new(t.Id,
            t.Name,
            t.Description,
            t.Status.ToString(),
            t.StartDate,
            t.EndDate,
            t.BonusConfig.Select(bc => bc.BonusQuestion?.QuestionKey ?? string.Empty).ToList());
}
