using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    // ── Bonus catalog ──────────────────────────────────────────────────────

    [HttpGet("bonus-catalog")]
    public async Task<IActionResult> ListCatalog(CancellationToken ct) =>
        Ok(await adminService.ListBonusCatalogAsync(ct));

    [HttpGet("bonus-catalog/{id:int}")]
    public async Task<IActionResult> GetCatalogItem(int id, CancellationToken ct) =>
        Ok(await adminService.GetBonusQuestionAsync(id, ct));

    // ── Tournament bonus configuration ─────────────────────────────────────

    [HttpGet("tournaments/{tournamentId:guid}/bonus-config")]
    public async Task<IActionResult> GetTournamentBonusConfig(Guid tournamentId, CancellationToken ct) =>
        Ok(await adminService.GetTournamentBonusConfigAsync(tournamentId, ct));

    [HttpPut("tournaments/{tournamentId:guid}/bonus-config")]
    public async Task<IActionResult> SetTournamentBonusConfig(
        Guid tournamentId,
        [FromBody] SetBonusConfigRequest request,
        CancellationToken ct)
    {
        await adminService.SetTournamentBonusConfigAsync(tournamentId, request, ct);
        return NoContent();
    }

    // ── Match bonus assignment ─────────────────────────────────────────────

    [HttpPost("matches/{matchId:guid}/assign-bonus-questions")]
    public async Task<IActionResult> AssignMatchBonusQuestions(Guid matchId, CancellationToken ct)
    {
        await adminService.AssignMatchBonusQuestionsAsync(matchId, ct);
        return NoContent();
    }
}
