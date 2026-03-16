using Predictly.Domain.Entities;

namespace Predictly.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
