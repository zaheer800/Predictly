using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Predictly.Core.Interfaces;

namespace Predictly.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>Admin-only: list all users.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await userService.ListAsync(ct));

    /// <summary>Get any user's public profile by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        Ok(await userService.GetByIdAsync(id, ct));

    /// <summary>Get the calling user's own profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await userService.GetByIdAsync(userId, ct));
    }
}
