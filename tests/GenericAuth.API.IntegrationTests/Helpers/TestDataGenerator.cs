using Bogus;
using GenericAuth.API.Controllers.V1;

namespace GenericAuth.API.IntegrationTests.Helpers;

/// <summary>
/// Generates realistic test data using Bogus library.
/// </summary>
public static class TestDataGenerator
{
    private static readonly Faker _faker = new();

    /// <summary>
    /// Generates a valid registration request with random data.
    /// </summary>
    public static RegisterRequest GenerateRegisterRequest(
        string? email = null,
        string? password = null)
    {
        return new RegisterRequest(
            FirstName: _faker.Name.FirstName(),
            LastName: _faker.Name.LastName(),
            Email: email ?? _faker.Internet.Email(),
            Password: password ?? "Test@123456"
        );
    }

    /// <summary>
    /// Generates a valid login request.
    /// </summary>
    public static LoginRequest GenerateLoginRequest(
        string email,
        string password,
        Guid? applicationId = null)
    {
        return new LoginRequest(email, password, applicationId);
    }

    /// <summary>
    /// Generates a create application role request.
    /// </summary>
    public static CreateApplicationRoleRequest GenerateCreateApplicationRoleRequest(
        string? name = null,
        string? description = null,
        bool isDefault = false)
    {
        return new CreateApplicationRoleRequest(
            Name: name ?? _faker.Name.JobTitle(),
            Description: description ?? _faker.Lorem.Sentence(),
            IsDefault: isDefault
        );
    }

    /// <summary>
    /// Generates a valid application code.
    /// </summary>
    public static string GenerateApplicationCode()
    {
        return _faker.Random.AlphaNumeric(8).ToUpper();
    }

    /// <summary>
    /// Generates a valid application name.
    /// </summary>
    public static string GenerateApplicationName()
    {
        return _faker.Company.CompanyName();
    }

    /// <summary>
    /// Generates a valid email address.
    /// </summary>
    public static string GenerateEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Generates a valid password that meets requirements.
    /// </summary>
    public static string GenerateValidPassword()
    {
        return "Test@" + _faker.Random.Number(100000, 999999);
    }

    /// <summary>
    /// Generates an invalid password (too short).
    /// </summary>
    public static string GenerateInvalidPassword()
    {
        return "weak";
    }

    /// <summary>
    /// Generates a role name.
    /// </summary>
    public static string GenerateRoleName()
    {
        return _faker.Name.JobTitle();
    }

    /// <summary>
    /// Generates a role description.
    /// </summary>
    public static string GenerateRoleDescription()
    {
        return _faker.Lorem.Sentence();
    }
}
