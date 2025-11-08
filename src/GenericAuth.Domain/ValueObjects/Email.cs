using System.Text.RegularExpressions;
using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty.");
        }

        if (!Regex.IsMatch(email, EmailPattern))
        {
            throw new DomainException($"Email '{email}' is not valid.");
        }

        return new Email(email.ToLowerInvariant().Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
