using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface IAdminService
{
    // Bonus catalog
    Task<IReadOnlyList<BonusQuestionDto>> ListBonusCatalogAsync(CancellationToken ct = default);
    Task<BonusQuestionDto> GetBonusQuestionAsync(int id, CancellationToken ct = default);

    // Tournament bonus configuration
    Task SetTournamentBonusConfigAsync(Guid tournamentId, SetBonusConfigRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BonusQuestionDto>> GetTournamentBonusConfigAsync(Guid tournamentId, CancellationToken ct = default);

    // Match bonus selection (3 random questions assigned to a match)
    Task AssignMatchBonusQuestionsAsync(Guid matchId, CancellationToken ct = default);
}
