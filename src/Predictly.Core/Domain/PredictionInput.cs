namespace Predictly.Core.Domain;

/// <summary>
/// All data needed to score a single user's prediction for a match.
/// </summary>
public class PredictionInput
{
    public Guid UserId { get; init; }

    /// <summary>Team ID the user predicted as winner.</summary>
    public string PredictedWinner { get; init; } = string.Empty;

    /// <summary>Actual match winner team ID.</summary>
    public string ActualWinner { get; init; } = string.Empty;

    /// <summary>Points awarded for a correct winner prediction.</summary>
    public int WinnerPoints { get; init; }

    /// <summary>The user's bonus answers (may be empty — bonus is optional).</summary>
    public IReadOnlyList<BonusAnswer> BonusAnswers { get; init; } = [];

    /// <summary>The actual results for each bonus question in the match.</summary>
    public IReadOnlyList<BonusResult> BonusResults { get; init; } = [];

    /// <summary>UTC timestamp at which this prediction was last finalized.</summary>
    public DateTimeOffset FinalizedAt { get; init; }
}
