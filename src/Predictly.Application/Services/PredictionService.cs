using Predictly.Application.DTOs.Predictions;
using Predictly.Application.Interfaces;
using Predictly.Domain.Entities;
using Predictly.Domain.Exceptions;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class PredictionService : IPredictionService
{
    private readonly IPredictionRepository _predictionRepo;
    private readonly IMatchRepository _matchRepo;

    public PredictionService(IPredictionRepository predictionRepo, IMatchRepository matchRepo)
    {
        _predictionRepo = predictionRepo;
        _matchRepo = matchRepo;
    }

    public async Task<PredictionResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prediction = await _predictionRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Prediction), id);
        return ToResponse(prediction);
    }

    public async Task<PredictionResponse?> GetByUserAndMatchAsync(int userId, int matchId, CancellationToken ct = default)
    {
        var prediction = await _predictionRepo.GetByUserAndMatchAsync(userId, matchId, ct);
        return prediction is null ? null : ToResponse(prediction);
    }

    public async Task<PredictionResponse> UpsertAsync(UpsertPredictionRequest request, int userId, CancellationToken ct = default)
    {
        var match = await _matchRepo.GetByIdAsync(request.MatchId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Match), request.MatchId);

        if (request.PredictedWinner != match.TeamHome && request.PredictedWinner != match.TeamAway)
            throw new BusinessRuleException("INVALID_WINNER", $"Predicted winner must be '{match.TeamHome}' or '{match.TeamAway}'.");

        var existing = await _predictionRepo.GetByUserAndMatchAsync(userId, request.MatchId, ct);

        if (existing is null)
        {
            // Create — pre-lock check (DB lock is definitive, this is a fast-fail UX check)
            if (match.MatchStartTime <= DateTime.UtcNow)
                throw new PredictionLockedException(request.MatchId);

            var prediction = new Prediction
            {
                UserId = userId,
                MatchId = request.MatchId,
                PredictedWinner = request.PredictedWinner,
                FinalizedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            MapBonusAnswers(prediction, request.BonusAnswers);
            return ToResponse(await _predictionRepo.AddAsync(prediction, ct));
        }
        else
        {
            // Update — DB-level lock enforcement
            var updated = await _predictionRepo.TryUpdateWithLockAsync(existing.Id, userId, request.PredictedWinner, ct);
            if (!updated)
                throw new PredictionLockedException(request.MatchId);

            // Re-fetch to return current state
            var refreshed = await _predictionRepo.GetByIdAsync(existing.Id, ct);
            return ToResponse(refreshed!);
        }
    }

    private static void MapBonusAnswers(Prediction prediction, IReadOnlyList<BonusAnswerInput>? inputs)
    {
        if (inputs is null) return;
        foreach (var input in inputs)
        {
            prediction.BonusAnswers.Add(new PredictionBonusAnswer
            {
                MatchBonusSelectionId = input.MatchBonusSelectionId,
                AnswerNumeric = input.AnswerNumeric,
                AnswerChoice = input.AnswerChoice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private static PredictionResponse ToResponse(Prediction p) =>
        new(p.Id, p.UserId, p.MatchId, p.PredictedWinner, p.FinalizedAt,
            p.BonusAnswers.Select(a => new BonusAnswerResponse(a.MatchBonusSelectionId, a.AnswerNumeric, a.AnswerChoice)).ToList());
}
