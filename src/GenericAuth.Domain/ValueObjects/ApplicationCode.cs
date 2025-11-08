using System.Text.RegularExpressions;
using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.ValueObjects;

/// <summary>
/// Represents a unique application code identifier.
/// </summary>
public sealed class ApplicationCode : ValueObject
{
    private const string CodePattern = @"^[a-zA-Z0-9_-]{3,50}$";

    public string Value { get; private set; }

    private ApplicationCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new application code with validation.
    /// </summary>
    /// <param name="code">The application code (3-50 alphanumeric characters, hyphens, or underscores)</param>
    /// <returns>A validated ApplicationCode instance</returns>
    /// <exception cref="DomainException">Thrown when code is invalid</exception>
    public static ApplicationCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Application code cannot be empty.");
        }

        if (!Regex.IsMatch(code, CodePattern))
        {
            throw new DomainException(
                "Application code must be 3-50 characters long and contain only letters, numbers, hyphens, or underscores.");
        }

        return new ApplicationCode(code.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ApplicationCode code) => code.Value;
}
