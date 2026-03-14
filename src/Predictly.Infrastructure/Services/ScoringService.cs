using Microsoft.EntityFrameworkCore;
using Predictly.Core.Domain;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Scoring;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Infrastructure.Services;

public class ScoringService(PredictlyDbContext db) : IScoringService
{
    public async Task ScoreMatchAsync(Guid matchId, CancellationToken ct = default)
    {
        var match = await db.Matches
            .Include(m => m.Predictions)
                .ThenInclude(p => p.BonusAnswers)
                    .ThenInclude(a => a.BonusQuestion)
            .Include(m => m.BonusSelections)
                .ThenInclude(s => s.Results)
            .FirstOrDefaultAsync(m => m.Id == matchId, ct)
            ?? throw new NotFoundException(nameof(Match), matchId);

        const int WinnerPoints = 10;

        var bonusResults = match.BonusSelections
            .SelectMany(s => s.Results)
            .Select(r => new BonusResult
            {
                QuestionType = Enum.Parse<BonusQuestionType>(r.Selection.BonusQuestion.QuestionType),
                ActualValue = r.ActualValue
            })
            .ToList();

        var predictionInputs = match.Predictions.Select(p => new PredictionInput
        {
            UserId = p.UserId,
            PredictedWinner = p.PredictedWinner,
            ActualWinner = match.WinnerTeam!,
            WinnerPoints = WinnerPoints,
            FinalizedAt = p.FinalizedAt,
            BonusAnswers = p.BonusAnswers
                .Select(a => new BonusAnswer
                {
                    QuestionType = Enum.Parse<BonusQuestionType>(a.BonusQuestion.QuestionType),
                    PredictedValue = a.AnswerValue,
                    MaxPoints = a.BonusQuestion.MaxPoints
                })
                .ToList()
        });

        var scores = ScoringEngine.ScoreMatch(predictionInputs, bonusResults);

        var predictionById = match.Predictions.ToDictionary(p => p.UserId);

        var scoreEntities = scores.Select(s => new PredictionScore
        {
            PredictionId = predictionById[s.UserId].Id,
            MatchId = matchId,
            UserId = s.UserId,
            WinnerScore = s.WinnerScore,
            BonusScore = s.BonusScores.Sum(),
            TotalScore = s.TotalScore,
            AnsweredAnyBonus = s.AnsweredAnyBonus
        });

        db.PredictionScores.AddRange(scoreEntities);
        await db.SaveChangesAsync(ct);
    }
}
