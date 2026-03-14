using Predictly.Core.Domain;
using Predictly.Core.Scoring;

namespace Predictly.Core.Tests.Scoring;

public class ScoringEngineTests
{
    // ---------------------
    // Winner scoring
    // ---------------------

    [Fact]
    public void ScoreWinner_CorrectPrediction_ReturnsWinnerPoints()
    {
        int score = ScoringEngine.ScoreWinner("TeamA", "TeamA", 10);
        Assert.Equal(10, score);
    }

    [Fact]
    public void ScoreWinner_IncorrectPrediction_ReturnsZero()
    {
        int score = ScoringEngine.ScoreWinner("TeamA", "TeamB", 10);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreWinner_CaseInsensitiveMatch_ReturnsWinnerPoints()
    {
        int score = ScoringEngine.ScoreWinner("teamA", "TEAMA", 10);
        Assert.Equal(10, score);
    }

    // ---------------------
    // Multiple-choice bonus
    // ---------------------

    [Fact]
    public void ScoreMultipleChoiceBonus_ExactMatch_ReturnsMaxPoints()
    {
        int score = ScoringEngine.ScoreMultipleChoiceBonus("Yes", "Yes", 5);
        Assert.Equal(5, score);
    }

    [Fact]
    public void ScoreMultipleChoiceBonus_CaseInsensitiveMatch_ReturnsMaxPoints()
    {
        int score = ScoringEngine.ScoreMultipleChoiceBonus("yes", "YES", 5);
        Assert.Equal(5, score);
    }

    [Fact]
    public void ScoreMultipleChoiceBonus_NoMatch_ReturnsZero()
    {
        int score = ScoringEngine.ScoreMultipleChoiceBonus("Yes", "No", 5);
        Assert.Equal(0, score);
    }

    // ---------------------
    // Numeric bonus
    // ---------------------

    [Fact]
    public void ScoreNumericBonus_ExactMatch_ReturnsMaxPoints()
    {
        // predicted == actual → percentage_diff = 0 → score = max_points
        int score = ScoringEngine.ScoreNumericBonus("100", "100", 10);
        Assert.Equal(10, score);
    }

    [Fact]
    public void ScoreNumericBonus_PredictedDoubleActual_ReturnsZero()
    {
        // predicted = 200, actual = 100 → diff = 100% → raw = 0
        int score = ScoringEngine.ScoreNumericBonus("200", "100", 10);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreNumericBonus_PredictedTripleActual_ReturnsZero()
    {
        // percentage_diff > 1 → clamped to 0
        int score = ScoringEngine.ScoreNumericBonus("300", "100", 10);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreNumericBonus_PartialCredit_ReturnsFlooredScore()
    {
        // predicted = 150, actual = 100 → diff = 0.5 → raw = 10 * 0.5 = 5
        int score = ScoringEngine.ScoreNumericBonus("150", "100", 10);
        Assert.Equal(5, score);
    }

    [Fact]
    public void ScoreNumericBonus_FloorApplied_TruncatesDecimal()
    {
        // predicted = 110, actual = 100 → diff = 0.1 → raw = 10 * 0.9 = 9.0
        int score = ScoringEngine.ScoreNumericBonus("110", "100", 10);
        Assert.Equal(9, score);
    }

    [Fact]
    public void ScoreNumericBonus_FloorApplied_TruncatesDecimalFraction()
    {
        // predicted = 115, actual = 100 → diff = 0.15 → raw = 10 * 0.85 = 8.5 → floor = 8
        int score = ScoringEngine.ScoreNumericBonus("115", "100", 10);
        Assert.Equal(8, score);
    }

    [Fact]
    public void ScoreNumericBonus_ActualZero_PredictedZero_ReturnsMaxPoints()
    {
        // Special case: actual == 0 and predicted == 0 → full points
        int score = ScoringEngine.ScoreNumericBonus("0", "0", 10);
        Assert.Equal(10, score);
    }

    [Fact]
    public void ScoreNumericBonus_ActualZero_PredictedNonZero_ReturnsZero()
    {
        // Special case: actual == 0 but predicted != 0 → 0
        int score = ScoringEngine.ScoreNumericBonus("5", "0", 10);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreNumericBonus_InvalidInput_ReturnsZero()
    {
        int score = ScoringEngine.ScoreNumericBonus("abc", "100", 10);
        Assert.Equal(0, score);
    }

    // ---------------------
    // Full prediction scoring
    // ---------------------

    [Fact]
    public void ScorePrediction_CorrectWinnerAndBonuses_ReturnsTotalScore()
    {
        var prediction = new PredictionInput
        {
            UserId = Guid.NewGuid(),
            PredictedWinner = "India",
            ActualWinner = "India",
            WinnerPoints = 10,
            BonusAnswers =
            [
                new BonusAnswer { QuestionType = BonusQuestionType.Numeric, PredictedValue = "150", MaxPoints = 10 },
                new BonusAnswer { QuestionType = BonusQuestionType.MultipleChoice, PredictedValue = "Yes", MaxPoints = 5 }
            ],
            FinalizedAt = DateTimeOffset.UtcNow
        };

        var bonusResults = new List<BonusResult>
        {
            new() { QuestionType = BonusQuestionType.Numeric, ActualValue = "100" },  // diff 50% → 5 pts
            new() { QuestionType = BonusQuestionType.MultipleChoice, ActualValue = "Yes" } // exact → 5 pts
        };

        PredictionScore score = ScoringEngine.ScorePrediction(prediction, bonusResults);

        Assert.Equal(10, score.WinnerScore);
        Assert.Equal([5, 5], score.BonusScores);
        Assert.Equal(20, score.TotalScore);
        Assert.True(score.AnsweredAnyBonus);
    }

    [Fact]
    public void ScorePrediction_NoBonusAnswers_OnlyWinnerCounted()
    {
        var prediction = new PredictionInput
        {
            UserId = Guid.NewGuid(),
            PredictedWinner = "Australia",
            ActualWinner = "Australia",
            WinnerPoints = 10,
            BonusAnswers = [],
            FinalizedAt = DateTimeOffset.UtcNow
        };

        var bonusResults = new List<BonusResult>
        {
            new() { QuestionType = BonusQuestionType.MultipleChoice, ActualValue = "Yes" }
        };

        PredictionScore score = ScoringEngine.ScorePrediction(prediction, bonusResults);

        Assert.Equal(10, score.WinnerScore);
        Assert.Empty(score.BonusScores);
        Assert.Equal(10, score.TotalScore);
        Assert.False(score.AnsweredAnyBonus);
    }

    [Fact]
    public void ScoreMatch_MultipleUsers_ScoresAllPredictions()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var predictions = new[]
        {
            new PredictionInput
            {
                UserId = userId1,
                PredictedWinner = "India",
                ActualWinner = "India",
                WinnerPoints = 10,
                BonusAnswers = [],
                FinalizedAt = DateTimeOffset.UtcNow
            },
            new PredictionInput
            {
                UserId = userId2,
                PredictedWinner = "Pakistan",
                ActualWinner = "India",
                WinnerPoints = 10,
                BonusAnswers = [],
                FinalizedAt = DateTimeOffset.UtcNow
            }
        };

        var bonusResults = Array.Empty<BonusResult>();

        var scores = ScoringEngine.ScoreMatch(predictions, bonusResults);

        Assert.Equal(2, scores.Count);
        Assert.Equal(10, scores.First(s => s.UserId == userId1).TotalScore);
        Assert.Equal(0, scores.First(s => s.UserId == userId2).TotalScore);
    }
}
