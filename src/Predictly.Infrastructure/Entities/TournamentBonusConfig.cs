namespace Predictly.Infrastructure.Entities;

/// <summary>
/// Which bonus question types are enabled for a given tournament.
/// Immutable once the tournament is activated.
/// </summary>
public class TournamentBonusConfig
{
    public Guid TournamentId { get; set; }
    public int BonusQuestionCatalogId { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public BonusQuestionCatalog BonusQuestion { get; set; } = null!;
}
