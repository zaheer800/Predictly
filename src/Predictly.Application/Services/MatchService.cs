using Predictly.Application.DTOs.Matches;
using Predictly.Application.Interfaces;
using Predictly.Domain.Entities;
using Predictly.Domain.Exceptions;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepo;

    public MatchService(IMatchRepository matchRepo) => _matchRepo = matchRepo;

    public async Task<MatchResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var match = await _matchRepo.GetForScoringAsync(id, ct)
            ?? throw new NotFoundException(nameof(Match), id);
        return MapToResponse(match);
    }

    public async Task<IReadOnlyList<MatchResponse>> GetByTournamentAsync(
        int tournamentId, CancellationToken ct = default)
    {
        var matches = await _matchRepo.GetScheduledWithSelectionsAsync(tournamentId, ct);
        return matches.Select(MapToResponse).ToList();
    }

    private static MatchResponse MapToResponse(Match m) =>
        new(m.Id,
            m.TournamentId,
            m.TeamHome,
            m.TeamAway,
            m.Venue,
            m.MatchStartTime,
            m.Status.ToString(),
            m.Winner,
            m.BonusSelections
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new BonusQuestionSlot(
                    s.Id,
                    s.BonusQuestion.QuestionKey,
                    s.BonusQuestion.QuestionTemplate,
                    s.BonusQuestion.AnswerType.ToString(),
                    s.BonusQuestion.Options,
                    s.BonusQuestion.MaxPoints))
                .ToList());
}
