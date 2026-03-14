using Predictly.Core.Models;

namespace Predictly.Core.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct = default);
}
