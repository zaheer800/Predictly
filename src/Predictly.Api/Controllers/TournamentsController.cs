using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/tournaments")]
[Authorize]
public class TournamentsController(ITournamentService tournamentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await tournamentService.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await tournamentService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequest request, CancellationToken ct)
    {
        var tournament = await tournamentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = tournament.Id }, tournament);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await tournamentService.ActivateAsync(id, ct);
        return NoContent();
    }
}
