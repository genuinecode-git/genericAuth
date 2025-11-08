using GenericAuth.Domain.Entities;

namespace GenericAuth.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
