using Predictly.Application.DTOs.Tournaments;

namespace Predictly.Application.Interfaces;

public interface ITournamentService
{
    Task<TournamentResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TournamentResponse>> GetAllAsync(CancellationToken ct = default);
    Task<TournamentResponse> CreateAsync(CreateTournamentRequest request, int createdBy, CancellationToken ct = default);
    Task<TournamentResponse> ActivateAsync(int id, CancellationToken ct = default);
    Task<TournamentResponse> CompleteAsync(int id, CancellationToken ct = default);
}
