using GenericAuth.Domain.Services;

namespace GenericAuth.Infrastructure.Services;

/// <summary>
/// Production implementation of IPasswordResetTokenStore that performs no operations.
/// This is the default implementation for production environments where plain-text
/// tokens should never be stored or retrieved for security reasons.
/// The actual password reset flow relies on hashed tokens stored in the database.
/// </summary>
public class NullPasswordResetTokenStore : IPasswordResetTokenStore
{
    /// <summary>
    /// No-op implementation - does not store the token.
    /// </summary>
    public void StoreToken(string email, string plainTextToken)
    {
        // No-op: In production, we don't store plain-text tokens
    }

    /// <summary>
    /// No-op implementation - always returns null.
    /// </summary>
    public string? GetToken(string email)
    {
        // No-op: In production, we don't retrieve plain-text tokens
        return null;
    }

    /// <summary>
    /// No-op implementation - does nothing.
    /// </summary>
    public void ClearToken(string email)
    {
        // No-op: In production, we don't manage plain-text tokens
    }
}
