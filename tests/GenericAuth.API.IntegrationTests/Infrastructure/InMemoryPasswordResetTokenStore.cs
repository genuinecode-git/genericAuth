using GenericAuth.Domain.Services;

namespace GenericAuth.API.IntegrationTests.Infrastructure;

/// <summary>
/// In-memory implementation of IPasswordResetTokenStore for integration testing.
/// Stores plain-text password reset tokens in memory to enable testing of password
/// reset flows without needing to retrieve hashed tokens from the database.
/// This should ONLY be used in test environments.
/// </summary>
public class InMemoryPasswordResetTokenStore : IPasswordResetTokenStore
{
    private readonly Dictionary<string, string> _tokens = new();

    /// <summary>
    /// Stores a plain-text password reset token for the specified email address.
    /// Email addresses are normalized to lowercase for case-insensitive lookups.
    /// </summary>
    public void StoreToken(string email, string plainTextToken)
    {
        _tokens[email.ToLowerInvariant()] = plainTextToken;
    }

    /// <summary>
    /// Retrieves the plain-text password reset token for the specified email address.
    /// Email addresses are normalized to lowercase for case-insensitive lookups.
    /// </summary>
    public string? GetToken(string email)
    {
        return _tokens.TryGetValue(email.ToLowerInvariant(), out var token) ? token : null;
    }

    /// <summary>
    /// Clears the stored token for the specified email address.
    /// Email addresses are normalized to lowercase for case-insensitive lookups.
    /// </summary>
    public void ClearToken(string email)
    {
        _tokens.Remove(email.ToLowerInvariant());
    }
}
