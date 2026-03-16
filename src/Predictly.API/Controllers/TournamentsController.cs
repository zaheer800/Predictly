using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.API.Models;
using Predictly.Application.DTOs.Tournaments;
using Predictly.Application.Interfaces;
using System.Security.Claims;

namespace Predictly.API.Controllers;

[ApiController]
[Route("api/v1/tournaments")]
[Authorize]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentsController(ITournamentService tournamentService) => _tournamentService = tournamentService;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TournamentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _tournamentService.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TournamentResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TournamentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _tournamentService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<TournamentResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TournamentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _tournamentService.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<TournamentResponse>.Ok(result));
    }

    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<TournamentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        var result = await _tournamentService.ActivateAsync(id, ct);
        return Ok(ApiResponse<TournamentResponse>.Ok(result));
    }
}
