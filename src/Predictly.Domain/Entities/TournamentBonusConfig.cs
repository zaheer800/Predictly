namespace Predictly.Domain.Entities;

public class TournamentBonusConfig
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int BonusQuestionId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = null!;
    public BonusQuestionCatalog BonusQuestion { get; set; } = null!;
}
