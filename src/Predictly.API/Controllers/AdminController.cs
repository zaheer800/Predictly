using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.API.Models;
using Predictly.Application.DTOs.Admin;
using Predictly.Application.Interfaces;
using System.Security.Claims;

namespace Predictly.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminUploadService _uploadService;

    public AdminController(IAdminUploadService uploadService) => _uploadService = uploadService;

    /// <summary>
    /// Accepts a .xlsx file with two sheets: MatchSetup and MatchResults.
    /// See docs/03_excel_upload_schema.md for the full specification.
    /// Max file size: 5 MB.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<UploadSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("MISSING_FILE", "No file provided."));

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object>.Fail("INVALID_FILE_TYPE", "Only .xlsx files are accepted."));

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await using var stream = file.OpenReadStream();
        var result = await _uploadService.ProcessUploadAsync(stream, userId, ct);
        return Ok(ApiResponse<UploadSummaryResponse>.Ok(result));
    }
}
