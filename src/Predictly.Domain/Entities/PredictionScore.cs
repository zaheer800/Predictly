namespace Predictly.Domain.Entities;

public class PredictionScore
{
    public int Id { get; set; }
    public int PredictionId { get; set; }
    public int UserId { get; set; }
    public int MatchId { get; set; }
    public int TournamentId { get; set; }
    public int WinnerScore { get; set; }
    public int BonusScore1 { get; set; }
    public int BonusScore2 { get; set; }
    public int BonusScore3 { get; set; }
    // TotalScore is a DB-generated computed column: winner_score + bonus_score_1 + bonus_score_2 + bonus_score_3
    public int TotalScore { get; private set; }
    public short BonusAnsweredCount { get; set; }
    public DateTime ScoredAt { get; set; }

    // Navigation
    public Prediction Prediction { get; set; } = null!;
    public User User { get; set; } = null!;
    public Match Match { get; set; } = null!;
    public Tournament Tournament { get; set; } = null!;
}
