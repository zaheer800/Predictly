using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize]
public class MatchesController(IMatchService matchService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await matchService.GetByIdAsync(id, ct));

    [HttpGet("tournament/{tournamentId:guid}")]
    public async Task<IActionResult> ListByTournament(Guid tournamentId, CancellationToken ct) =>
        Ok(await matchService.ListByTournamentAsync(tournamentId, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request, CancellationToken ct)
    {
        var match = await matchService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = match.Id }, match);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteMatchRequest request, CancellationToken ct)
    {
        await matchService.CompleteAsync(id, request, ct);
        return NoContent();
    }
}
