namespace Predictly.Core.Domain;

/// <summary>
/// Computed score breakdown for a single user's match prediction.
/// </summary>
public class PredictionScore
{
    public Guid UserId { get; init; }

    public int WinnerScore { get; init; }

    public IReadOnlyList<int> BonusScores { get; init; } = [];

    public int TotalScore => WinnerScore + BonusScores.Sum();

    public bool AnsweredAnyBonus => BonusScores.Count > 0;
}
