using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.ValueObjects;

public sealed class Password : ValueObject
{
    public string Hash { get; private set; } = string.Empty;

    // Required for EF Core
    private Password()
    {
    }

    private Password(string hash)
    {
        Hash = hash;
    }

    public static Password Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        return new Password(hash);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    public override string ToString() => "***PROTECTED***";
}
