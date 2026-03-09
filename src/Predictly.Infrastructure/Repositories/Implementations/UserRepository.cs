using Microsoft.EntityFrameworkCore;
using Predictly.Domain.Entities;
using Predictly.Infrastructure.Persistence;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Infrastructure.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly PredictlyDbContext _db;

    public UserRepository(PredictlyDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower(), ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == email.ToLower(), ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Username == username.ToLower(), ct);
}
