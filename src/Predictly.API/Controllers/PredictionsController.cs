using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.API.Models;
using Predictly.Application.DTOs.Predictions;
using Predictly.Application.Interfaces;
using System.Security.Claims;

namespace Predictly.API.Controllers;

[ApiController]
[Route("api/v1/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionsController(IPredictionService predictionService) => _predictionService = predictionService;

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PredictionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _predictionService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<PredictionResponse>.Ok(result));
    }

    [HttpGet("my/{matchId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PredictionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPrediction(int matchId, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _predictionService.GetByUserAndMatchAsync(userId, matchId, ct);
        return result is null ? NotFound(ApiResponse<PredictionResponse>.Fail("NOT_FOUND", "No prediction found.")) : Ok(ApiResponse<PredictionResponse>.Ok(result));
    }

    /// <summary>
    /// Create or update a prediction for a match.
    /// Lock is enforced at the database level — 409 returned if match has started.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<PredictionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Upsert([FromBody] UpsertPredictionRequest request, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _predictionService.UpsertAsync(request, userId, ct);
        return Ok(ApiResponse<PredictionResponse>.Ok(result));
    }
}
