using System.Security.Cryptography;
using System.Text;
using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.ValueObjects;

/// <summary>
/// Represents an API key for application authentication.
/// API keys are hashed for security (similar to passwords).
/// </summary>
public sealed class ApiKey : ValueObject
{
    private const int KeyLengthBytes = 32; // 256 bits

    /// <summary>
    /// The hashed API key value (stored in database).
    /// </summary>
    public string HashedValue { get; private set; }

    /// <summary>
    /// When the API key was generated.
    /// </summary>
    public DateTime GeneratedAt { get; private set; }

    private ApiKey(string hashedValue, DateTime generatedAt)
    {
        HashedValue = hashedValue;
        GeneratedAt = generatedAt;
    }

    /// <summary>
    /// Generates a new cryptographically secure API key.
    /// </summary>
    /// <returns>Tuple containing the ApiKey object (with hashed value) and the plain text key (for one-time display)</returns>
    public static (ApiKey apiKey, string plainKey) Generate()
    {
        var bytes = new byte[KeyLengthBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        // Create URL-safe base64 string
        var plainKey = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var hashedKey = HashApiKey(plainKey);
        var apiKey = new ApiKey(hashedKey, DateTime.UtcNow);

        return (apiKey, plainKey);
    }

    /// <summary>
    /// Creates an ApiKey from an already hashed value (for EF Core rehydration).
    /// </summary>
    /// <param name="hashedValue">The hashed API key</param>
    /// <param name="generatedAt">When it was generated</param>
    /// <returns>ApiKey instance</returns>
    public static ApiKey CreateFromHash(string hashedValue, DateTime generatedAt)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
        {
            throw new DomainException("Hashed API key cannot be empty.");
        }

        return new ApiKey(hashedValue, generatedAt);
    }

    /// <summary>
    /// Validates a plain text API key against the stored hash.
    /// </summary>
    /// <param name="plainApiKey">The plain text API key to validate</param>
    /// <returns>True if the key matches, false otherwise</returns>
    public bool Validate(string plainApiKey)
    {
        if (string.IsNullOrWhiteSpace(plainApiKey))
        {
            return false;
        }

        var hashedInput = HashApiKey(plainApiKey);
        return HashedValue.Equals(hashedInput, StringComparison.Ordinal);
    }

    /// <summary>
    /// Hashes an API key using SHA-256.
    /// </summary>
    private static string HashApiKey(string plainKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plainKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HashedValue;
        yield return GeneratedAt;
    }

    public override string ToString() => $"ApiKey(Generated: {GeneratedAt:yyyy-MM-dd})";
}
