using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.API.Models;
using Predictly.Application.DTOs.Leaderboard;
using Predictly.Application.Interfaces;

namespace Predictly.API.Controllers;

[ApiController]
[Route("api/v1/leaderboard")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService) => _leaderboardService = leaderboardService;

    [HttpGet("tournament/{tournamentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TournamentLeaderboardEntry>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTournament(int tournamentId, CancellationToken ct)
    {
        var result = await _leaderboardService.GetTournamentLeaderboardAsync(tournamentId, ct);
        return Ok(ApiResponse<IReadOnlyList<TournamentLeaderboardEntry>>.Ok(result));
    }

    [HttpGet("global")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GlobalLeaderboardEntry>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobal(CancellationToken ct)
    {
        var result = await _leaderboardService.GetGlobalLeaderboardAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<GlobalLeaderboardEntry>>.Ok(result));
    }
}
