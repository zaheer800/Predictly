using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.API.Models;
using Predictly.Application.DTOs.Matches;
using Predictly.Application.Interfaces;

namespace Predictly.API.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService) => _matchService = matchService;

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<MatchResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _matchService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<MatchResponse>.Ok(result));
    }

    [HttpGet("by-tournament/{tournamentId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MatchResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTournament(int tournamentId, CancellationToken ct)
    {
        var result = await _matchService.GetByTournamentAsync(tournamentId, ct);
        return Ok(ApiResponse<IReadOnlyList<MatchResponse>>.Ok(result));
    }
}
