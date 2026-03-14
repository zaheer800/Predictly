using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/leaderboard")]
[Authorize]
public class LeaderboardController(ILeaderboardService leaderboardService) : ControllerBase
{
    [HttpGet("tournament/{tournamentId:guid}")]
    public async Task<IActionResult> Tournament(Guid tournamentId, CancellationToken ct) =>
        Ok(await leaderboardService.GetTournamentLeaderboardAsync(tournamentId, ct));

    [HttpGet("global")]
    public async Task<IActionResult> Global(CancellationToken ct) =>
        Ok(await leaderboardService.GetGlobalLeaderboardAsync(ct));
}
