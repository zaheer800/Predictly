using Microsoft.EntityFrameworkCore;
using Predictly.Core.Exceptions;
using Predictly.Core.Interfaces;
using Predictly.Core.Models;
using Predictly.Infrastructure.Entities;
using Predictly.Infrastructure.Persistence;

namespace Predictly.Infrastructure.Services;

public class UserService(PredictlyDbContext db) : IUserService
{
    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(User), id);
        return ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct = default)
    {
        var users = await db.Users.OrderBy(u => u.Username).ToListAsync(ct);
        return users.Select(ToDto).ToList();
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Username, u.Email, u.Role, u.CreatedAt);
}
