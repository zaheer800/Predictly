namespace Predictly.Domain.Entities;

public class MatchBonusResult
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int MatchBonusSelectionId { get; set; }
    public decimal? ActualNumeric { get; set; }
    public string? ActualChoice { get; set; }
    public int SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; }

    // Navigation
    public Match Match { get; set; } = null!;
    public MatchBonusSelection MatchBonusSelection { get; set; } = null!;
    public User SubmittedByUser { get; set; } = null!;
}
