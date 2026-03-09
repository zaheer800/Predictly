using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;

namespace Predictly.Infrastructure.Persistence;

/// <summary>
/// Seeds the bonus_question_catalog with the 10 predefined cricket question types.
/// Run once on first deployment or via migration data seed.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedBonusQuestionCatalogAsync(PredictlyDbContext db, CancellationToken ct = default)
    {
        if (await db.BonusQuestionCatalog.AnyAsync(ct))
            return;

        var questions = new List<BonusQuestionCatalog>
        {
            new() { QuestionKey = "total_sixes",             QuestionTemplate = "How many sixes will be hit in total during this match?",                             AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "total_fours",             QuestionTemplate = "How many fours will be hit in total during this match?",                             AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "total_match_runs",        QuestionTemplate = "What will be the total combined runs scored by both teams?",                         AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "winning_margin_runs",     QuestionTemplate = "By how many runs will the winning team win (applicable when defending team wins)?",  AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "winning_margin_wickets",  QuestionTemplate = "By how many wickets will the winning team win (applicable when chasing team wins)?", AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "top_scorer_team",         QuestionTemplate = "Which team will score the most runs?",                                               AnswerType = AnswerType.MultipleChoice,  Options = ["Home", "Away"],      MaxPoints = 5,  ScoringRule = ScoringRule.ExactMatch,      IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "toss_winner_bats_or_bowls", QuestionTemplate = "What will the toss-winning team choose to do?",                                   AnswerType = AnswerType.MultipleChoice,  Options = ["Bat", "Bowl"],       MaxPoints = 5,  ScoringRule = ScoringRule.ExactMatch,      IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "match_goes_to_super_over",QuestionTemplate = "Will this match go to a Super Over?",                                               AnswerType = AnswerType.MultipleChoice,  Options = ["Yes", "No"],         MaxPoints = 5,  ScoringRule = ScoringRule.ExactMatch,      IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "total_wickets_fallen",    QuestionTemplate = "How many wickets will fall across both innings in total?",                           AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { QuestionKey = "highest_individual_score",QuestionTemplate = "What will be the highest individual score by any batter in the match?",             AnswerType = AnswerType.Numeric,         Options = null,                 MaxPoints = 10, ScoringRule = ScoringRule.PercentageBased, IsActive = true, CreatedAt = DateTime.UtcNow },
        };

        db.BonusQuestionCatalog.AddRange(questions);
        await db.SaveChangesAsync(ct);
    }
}
