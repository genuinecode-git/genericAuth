using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenericAuth.API.Controllers.V1;
using GenericAuth.API.IntegrationTests.Helpers;
using GenericAuth.Application.Features.Applications.Commands.CreateApplication;
using GenericAuth.Domain.Entities;
using GenericAuth.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.API.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests providing common setup and helper methods.
/// </summary>
public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    // Default test credentials
    protected const string DefaultAuthAdminEmail = "admin@genericauth.com";
    protected const string DefaultAuthAdminPassword = "Admin@123";

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public virtual async Task InitializeAsync()
    {
        // Reset database before each test
        await _factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    #region User Creation Helpers

    /// <summary>
    /// Creates and returns the default Auth Admin user credentials.
    /// The user is created by the database seeder.
    /// </summary>
    protected (string Email, string Password) GetAuthAdminCredentials()
    {
        return (DefaultAuthAdminEmail, DefaultAuthAdminPassword);
    }

    /// <summary>
    /// Creates a new regular user and returns their credentials.
    /// </summary>
    protected async Task<(Guid UserId, string Email, string Password)> CreateRegularUserAsync(
        string? email = null,
        string? password = null)
    {
        var testEmail = email ?? TestDataGenerator.GenerateEmail();
        var testPassword = password ?? TestDataGenerator.GenerateValidPassword();

        var registerRequest = new RegisterRequest(
            FirstName: "Test",
            LastName: "User",
            Email: testEmail,
            Password: testPassword
        );

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserRegistrationResponse>>(content, _jsonOptions);

        return (result!.Data.UserId, testEmail, testPassword);
    }

    #endregion

    #region Application Creation Helpers

    /// <summary>
    /// Creates a test application and returns its details.
    /// Requires Auth Admin authentication.
    /// </summary>
    protected async Task<ApplicationResponse> CreateApplicationAsync(
        string? code = null,
        string? name = null)
    {
        var appCode = code ?? TestDataGenerator.GenerateApplicationCode();
        var appName = name ?? TestDataGenerator.GenerateApplicationName();

        using var scope = _factory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new CreateApplicationCommand(appName, appCode, new List<CreateApplicationRoleDto>());
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create application: {string.Join(", ", result.Errors)}");
        }

        // Get the application ID from database
        var appId = await WithDbContextAsync(async context =>
        {
            var app = await context.Applications.FirstOrDefaultAsync(a => a.Code.Value == result.Value.Code);
            return app!.Id;
        });

        return new ApplicationResponse(
            appId,
            result.Value.Code,
            appName
        );
    }

    #endregion

    #region Role Creation Helpers

    /// <summary>
    /// Creates an application role directly in the database.
    /// </summary>
    protected async Task<Guid> CreateApplicationRoleAsync(
        Guid applicationId,
        string name,
        bool isDefault = false)
    {
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = ApplicationRole.Create(applicationId, name, $"Description for {name}");
        if (isDefault)
        {
            role.SetAsDefault();
        }

        await context.ApplicationRoles.AddAsync(role);
        await context.SaveChangesAsync();

        return role.Id;
    }

    #endregion

    #region Authentication Helpers

    /// <summary>
    /// Logs in and returns access token and refresh token.
    /// </summary>
    protected async Task<(string AccessToken, string RefreshToken)> GetAuthTokensAsync(
        string email,
        string password,
        Guid? applicationId = null)
    {
        var loginRequest = new LoginRequest(email, password, applicationId);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(content, _jsonOptions);

        return (result!.Data.AccessToken, result.Data.RefreshToken);
    }

    /// <summary>
    /// Logs in as Auth Admin and sets the authorization header.
    /// </summary>
    protected async Task AuthenticateAsAuthAdminAsync()
    {
        var (accessToken, _) = await GetAuthTokensAsync(
            DefaultAuthAdminEmail,
            DefaultAuthAdminPassword);

        SetAuthHeader(accessToken);
    }

    /// <summary>
    /// Sets the Authorization header with a bearer token.
    /// </summary>
    protected void SetAuthHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the Authorization header.
    /// </summary>
    protected void ClearAuthHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region HTTP Helper Methods

    /// <summary>
    /// Performs a GET request and deserializes the response.
    /// </summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await _client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Performs a POST request with JSON body.
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<TRequest>(string url, TRequest request)
    {
        return await _client.PostAsJsonAsync(url, request);
    }

    /// <summary>
    /// Performs a PUT request with JSON body.
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<TRequest>(string url, TRequest request)
    {
        return await _client.PutAsJsonAsync(url, request);
    }

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await _client.DeleteAsync(url);
    }

    #endregion

    #region Database Access Helpers

    /// <summary>
    /// Gets a database context for direct database operations.
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        var scope = _factory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Executes an action with a database context.
    /// </summary>
    protected async Task<T> WithDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(context);
    }

    /// <summary>
    /// Executes an action with a database context (void return).
    /// </summary>
    protected async Task WithDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    #endregion
}

#region Response Models

/// <summary>
/// Standard API response wrapper.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; } = default!;
    public string[]? Errors { get; set; }
}

/// <summary>
/// User registration response.
/// </summary>
public class UserRegistrationResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
}

/// <summary>
/// Login response.
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Application response.
/// </summary>
public record ApplicationResponse(
    Guid Id,
    string Code,
    string Name);

#endregion
