using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.ValueObjects;

public sealed class RefreshToken : ValueObject
{
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public static RefreshToken Create(string token, int validityInDays = 7)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Refresh token cannot be empty.");
        }

        if (validityInDays <= 0)
        {
            throw new DomainException("Validity period must be positive.");
        }

        return new RefreshToken(token, DateTime.UtcNow.AddDays(validityInDays));
    }

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
        {
            throw new DomainException("Token is already revoked.");
        }

        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Token;
        yield return ExpiresAt;
        yield return CreatedAt;
    }
}
