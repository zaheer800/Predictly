using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface ITournamentService
{
    Task<TournamentDto> CreateAsync(CreateTournamentRequest request, CancellationToken ct = default);
    Task<TournamentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TournamentDto>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Activates a tournament, locking its bonus configuration.
    /// Throws if already active or completed.
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken ct = default);
}
