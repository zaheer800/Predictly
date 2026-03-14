using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/predictions")]
[Authorize]
public class PredictionsController(IPredictionService predictionService) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertPredictionRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var prediction = await predictionService.UpsertAsync(userId, request, ct);
        return Ok(prediction);
    }

    [HttpGet("my/{matchId:guid}")]
    public async Task<IActionResult> GetMine(Guid matchId, CancellationToken ct)
    {
        var userId = GetUserId();
        var prediction = await predictionService.GetByUserAndMatchAsync(userId, matchId, ct);
        return prediction is null ? NotFound() : Ok(prediction);
    }

    [HttpGet("match/{matchId:guid}")]
    public async Task<IActionResult> ListByMatch(Guid matchId, CancellationToken ct) =>
        Ok(await predictionService.ListByMatchAsync(matchId, ct));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
