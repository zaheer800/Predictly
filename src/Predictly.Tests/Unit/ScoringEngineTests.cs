using FluentAssertions;
using Predictly.Application.Services;
using Predictly.Domain.Constants;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;
using Xunit;

namespace Predictly.Tests.Unit;

/// <summary>
/// Acting as QA (BMAD): Exhaustive tests for ScoringEngine.
/// Covers all cases from CLAUDE.md §12.2 and docs/02_bonus_question_catalog.md.
/// </summary>
public class ScoringEngineTests
{
    private readonly ScoringEngine _engine = new();

    // ── Winner Score ──────────────────────────────────────────────────────────

    [Fact]
    public void CalculateWinnerScore_CorrectWinner_ReturnsWinnerPoints()
    {
        _engine.CalculateWinnerScore("India", "India").Should().Be(ScoringConstants.WinnerPoints);
    }

    [Fact]
    public void CalculateWinnerScore_WrongWinner_ReturnsZero()
    {
        _engine.CalculateWinnerScore("Australia", "India").Should().Be(0);
    }

    [Fact]
    public void CalculateWinnerScore_NullPredicted_ReturnsZero()
    {
        _engine.CalculateWinnerScore(null!, "India").Should().Be(0);
    }

    [Fact]
    public void CalculateWinnerScore_EmptyPredicted_ReturnsZero()
    {
        _engine.CalculateWinnerScore("", "India").Should().Be(0);
    }

    [Fact]
    public void CalculateWinnerScore_CaseSensitive_WrongCase_ReturnsZero()
    {
        // Teams are stored as entered — "india" != "India"
        _engine.CalculateWinnerScore("india", "India").Should().Be(0);
    }

    // ── Numeric Bonus: Normal Cases ───────────────────────────────────────────

    [Fact]
    public void CalculateNumericBonus_ExactMatch_ReturnsFullPoints()
    {
        _engine.CalculateNumericBonusScore(predicted: 15, actual: 15, maxPoints: 10).Should().Be(10);
    }

    [Fact]
    public void CalculateNumericBonus_CloseGuess_ReturnsPartialPoints()
    {
        // |14 - 15| / 15 = 0.0667 → 10 × (1 - 0.0667) = 9.33 → floor = 9
        _engine.CalculateNumericBonusScore(predicted: 14, actual: 15, maxPoints: 10).Should().Be(9);
    }

    [Fact]
    public void CalculateNumericBonus_FarOff_ReturnsZero()
    {
        // |100 - 10| / 10 = 9.0 → raw = 10 × (1 - 9) = -80 → clamped to 0
        _engine.CalculateNumericBonusScore(predicted: 100, actual: 10, maxPoints: 10).Should().Be(0);
    }

    [Fact]
    public void CalculateNumericBonus_OverDoubleActual_ReturnsZero()
    {
        // |20 - 10| / 10 = 1.0 → raw = 10 × 0 = 0
        _engine.CalculateNumericBonusScore(predicted: 20, actual: 10, maxPoints: 10).Should().Be(0);
    }

    [Fact]
    public void CalculateNumericBonus_PredictedZero_ActualNonZero_ReturnsZero()
    {
        _engine.CalculateNumericBonusScore(predicted: 0, actual: 15, maxPoints: 10).Should().Be(0);
    }

    [Fact]
    public void CalculateNumericBonus_FloorApplied_NotRounded()
    {
        // |9 - 10| / 10 = 0.1 → 10 × 0.9 = 9.0 (exact, floor = 9)
        _engine.CalculateNumericBonusScore(predicted: 9, actual: 10, maxPoints: 10).Should().Be(9);

        // |12 - 15| / 15 = 0.2 → 10 × 0.8 = 8.0 (exact, floor = 8)
        _engine.CalculateNumericBonusScore(predicted: 12, actual: 15, maxPoints: 10).Should().Be(8);
    }

    // ── Numeric Bonus: actual == 0 special cases ──────────────────────────────

    [Fact]
    public void CalculateNumericBonus_ActualZero_PredictedZero_ReturnsFullPoints()
    {
        _engine.CalculateNumericBonusScore(predicted: 0, actual: 0, maxPoints: 10).Should().Be(10);
    }

    [Fact]
    public void CalculateNumericBonus_ActualZero_PredictedNonZero_ReturnsZero()
    {
        _engine.CalculateNumericBonusScore(predicted: 5, actual: 0, maxPoints: 10).Should().Be(0);
    }

    [Fact]
    public void CalculateNumericBonus_ActualZero_PredictedPositive_ReturnsZero()
    {
        _engine.CalculateNumericBonusScore(predicted: 1, actual: 0, maxPoints: 10).Should().Be(0);
    }

    // ── Numeric Bonus: decimal values ─────────────────────────────────────────

    [Fact]
    public void CalculateNumericBonus_DecimalActual_FloorApplied()
    {
        // |87.0 - 90.5| / 90.5 ≈ 0.03867 → 10 × 0.96132 ≈ 9.613 → floor = 9
        _engine.CalculateNumericBonusScore(predicted: 87.0m, actual: 90.5m, maxPoints: 10).Should().Be(9);
    }

    // ── Multiple Choice ───────────────────────────────────────────────────────

    [Fact]
    public void CalculateMultipleChoice_ExactMatch_ReturnsFullPoints()
    {
        _engine.CalculateMultipleChoiceScore("Bat", "Bat", maxPoints: 5).Should().Be(5);
    }

    [Fact]
    public void CalculateMultipleChoice_WrongAnswer_ReturnsZero()
    {
        _engine.CalculateMultipleChoiceScore("Bat", "Bowl", maxPoints: 5).Should().Be(0);
    }

    [Fact]
    public void CalculateMultipleChoice_NoAnswer_ReturnsZero()
    {
        _engine.CalculateMultipleChoiceScore("", "Yes", maxPoints: 5).Should().Be(0);
    }

    [Fact]
    public void CalculateMultipleChoice_NullAnswer_ReturnsZero()
    {
        _engine.CalculateMultipleChoiceScore(null!, "Yes", maxPoints: 5).Should().Be(0);
    }

    [Fact]
    public void CalculateMultipleChoice_CaseSensitive_WrongCase_ReturnsZero()
    {
        _engine.CalculateMultipleChoiceScore("yes", "Yes", maxPoints: 5).Should().Be(0);
    }

    // ── ComputePredictionScore: full integration ──────────────────────────────

    [Fact]
    public void ComputePredictionScore_AllCorrect_ReturnsTotalWithAllBonuses()
    {
        var (prediction, selections, results, catalogLookup) = BuildMatchScenario(
            predictedWinner: "India",
            actualWinner: "India",
            bonusAnswers: [(15, null), (0, null), (null, "Bat")],
            bonusActuals: [(15, null), (0, null), (null, "Bat")]
        );

        var score = _engine.ComputePredictionScore(prediction, "India", selections, results, tournamentId: 1);

        score.WinnerScore.Should().Be(ScoringConstants.WinnerPoints);
        score.BonusScore1.Should().Be(10);  // exact numeric match
        score.BonusScore2.Should().Be(10);  // actual=0, predicted=0
        score.BonusScore3.Should().Be(5);   // exact choice match
        score.BonusAnsweredCount.Should().Be(3);
    }

    [Fact]
    public void ComputePredictionScore_WrongWinner_ZeroWinnerScore()
    {
        var (prediction, selections, results, _) = BuildMatchScenario(
            predictedWinner: "Australia",
            actualWinner: "India",
            bonusAnswers: [(15, null), (0, null), (null, "Bat")],
            bonusActuals: [(15, null), (0, null), (null, "Bat")]
        );

        var score = _engine.ComputePredictionScore(prediction, "India", selections, results, tournamentId: 1);

        score.WinnerScore.Should().Be(0);
        score.BonusScore1.Should().Be(10);
    }

    [Fact]
    public void ComputePredictionScore_NoBonusAnswers_ZeroBonusScores()
    {
        var prediction = new Prediction
        {
            Id = 1, UserId = 1, MatchId = 1,
            PredictedWinner = "India",
            FinalizedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
            BonusAnswers = []
        };

        var (_, selections, results, _) = BuildMatchScenario(
            "India", "India",
            [(15, null), (0, null), (null, "Bat")],
            [(15, null), (0, null), (null, "Bat")]
        );

        var score = _engine.ComputePredictionScore(prediction, "India", selections, results, tournamentId: 1);

        score.WinnerScore.Should().Be(ScoringConstants.WinnerPoints);
        score.BonusScore1.Should().Be(0);
        score.BonusScore2.Should().Be(0);
        score.BonusScore3.Should().Be(0);
        score.BonusAnsweredCount.Should().Be(0);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (Prediction, List<MatchBonusSelection>, List<MatchBonusResult>, object)
        BuildMatchScenario(
            string predictedWinner,
            string actualWinner,
            (decimal? numeric, string? choice)[] bonusAnswers,
            (decimal? numeric, string? choice)[] bonusActuals)
    {
        var numericQ = new BonusQuestionCatalog { Id = 1, QuestionKey = "total_sixes", QuestionTemplate = "...", AnswerType = AnswerType.Numeric, MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased };
        var numericQ2 = new BonusQuestionCatalog { Id = 2, QuestionKey = "winning_margin_runs", QuestionTemplate = "...", AnswerType = AnswerType.Numeric, MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased };
        var choiceQ = new BonusQuestionCatalog { Id = 3, QuestionKey = "toss_winner_bats_or_bowls", QuestionTemplate = "...", AnswerType = AnswerType.MultipleChoice, Options = ["Bat", "Bowl"], MaxPoints = 5, ScoringRule = ScoringRule.ExactMatch };

        var selections = new List<MatchBonusSelection>
        {
            new() { Id = 101, MatchId = 1, BonusQuestionId = 1, DisplayOrder = 1, BonusQuestion = numericQ },
            new() { Id = 102, MatchId = 1, BonusQuestionId = 2, DisplayOrder = 2, BonusQuestion = numericQ2 },
            new() { Id = 103, MatchId = 1, BonusQuestionId = 3, DisplayOrder = 3, BonusQuestion = choiceQ },
        };

        var results = new List<MatchBonusResult>
        {
            new() { Id = 1, MatchId = 1, MatchBonusSelectionId = 101, ActualNumeric = bonusActuals[0].numeric, ActualChoice = bonusActuals[0].choice, SubmittedBy = 1, SubmittedAt = DateTime.UtcNow },
            new() { Id = 2, MatchId = 1, MatchBonusSelectionId = 102, ActualNumeric = bonusActuals[1].numeric, ActualChoice = bonusActuals[1].choice, SubmittedBy = 1, SubmittedAt = DateTime.UtcNow },
            new() { Id = 3, MatchId = 1, MatchBonusSelectionId = 103, ActualNumeric = bonusActuals[2].numeric, ActualChoice = bonusActuals[2].choice, SubmittedBy = 1, SubmittedAt = DateTime.UtcNow },
        };

        var prediction = new Prediction
        {
            Id = 1, UserId = 1, MatchId = 1,
            PredictedWinner = predictedWinner,
            FinalizedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
            BonusAnswers = bonusAnswers.Select((a, i) => new PredictionBonusAnswer
            {
                Id = i + 1, PredictionId = 1,
                MatchBonusSelectionId = 101 + i,
                AnswerNumeric = a.numeric, AnswerChoice = a.choice,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        return (prediction, selections, results, new object());
    }
}
