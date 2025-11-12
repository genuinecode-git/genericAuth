namespace GenericAuth.Domain.Services;

/// <summary>
/// Interface for storing and retrieving plain-text password reset tokens.
/// This is primarily used for testing purposes to avoid the need to retrieve
/// hashed tokens from the database when testing password reset flows.
/// Production implementations should be no-op for security reasons.
/// </summary>
public interface IPasswordResetTokenStore
{
    /// <summary>
    /// Stores a plain-text password reset token associated with an email address.
    /// </summary>
    /// <param name="email">The user's email address (case-insensitive).</param>
    /// <param name="plainTextToken">The plain-text reset token.</param>
    void StoreToken(string email, string plainTextToken);

    /// <summary>
    /// Retrieves a plain-text password reset token for the specified email address.
    /// </summary>
    /// <param name="email">The user's email address (case-insensitive).</param>
    /// <returns>The plain-text token if found, otherwise null.</returns>
    string? GetToken(string email);

    /// <summary>
    /// Clears the stored token for the specified email address.
    /// </summary>
    /// <param name="email">The user's email address (case-insensitive).</param>
    void ClearToken(string email);
}
