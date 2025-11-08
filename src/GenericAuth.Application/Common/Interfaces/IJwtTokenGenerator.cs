using GenericAuth.Domain.Entities;

namespace GenericAuth.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for a user with system-level roles (for Auth Admins).
    /// </summary>
    string GenerateToken(User user, IEnumerable<string> roles);

    /// <summary>
    /// Generates a JWT token for a user with application context and application-specific role.
    /// Used for regular users authenticating to a specific application.
    /// </summary>
    string GenerateApplicationScopedToken(
        User user,
        Guid applicationId,
        string applicationCode,
        string applicationRoleName,
        IEnumerable<string> permissions);

    string GenerateRefreshToken();
}
