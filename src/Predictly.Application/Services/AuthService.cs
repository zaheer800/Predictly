using System.Security.Cryptography;
using Predictly.Application.DTOs.Auth;
using Predictly.Application.Interfaces;
using Predictly.Domain.Entities;
using Predictly.Domain.Enums;
using Predictly.Domain.Exceptions;
using Predictly.Infrastructure.Repositories.Interfaces;

namespace Predictly.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(IUserRepository userRepo, IJwtTokenGenerator jwtGenerator)
    {
        _userRepo = userRepo;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _userRepo.ExistsByEmailAsync(request.Email, ct))
            throw new BusinessRuleException("EMAIL_TAKEN", "Email is already registered.");

        if (await _userRepo.ExistsByUsernameAsync(request.Username, ct))
            throw new BusinessRuleException("USERNAME_TAKEN", "Username is already taken.");

        var user = new User
        {
            Username     = request.Username.ToLower(),
            Email        = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            DisplayName  = request.DisplayName,
            Role         = UserRole.User,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user, ct);

        return new AuthResponse(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Role.ToString(),
            _jwtGenerator.Generate(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new BusinessRuleException("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new BusinessRuleException("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!user.IsActive)
            throw new BusinessRuleException("ACCOUNT_INACTIVE", "Your account is inactive.");

        return new AuthResponse(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Role.ToString(),
            _jwtGenerator.Generate(user));
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt         = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        var actualHash   = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 100_000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
