using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericAuth.API.Controllers.V1;
using GenericAuth.API.IntegrationTests.Helpers;
using GenericAuth.API.IntegrationTests.Infrastructure;
using GenericAuth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.API.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AuthController endpoints.
/// Tests authentication flows including registration, login, token refresh, logout, and password reset.
/// </summary>
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = TestDataGenerator.GenerateRegisterRequest();

        // Act
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserRegistrationResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(request.Email);
        result.Data.FirstName.Should().Be(request.FirstName);
        result.Data.LastName.Should().Be(request.LastName);
        result.Data.UserType.Should().Be("RegularUser");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = TestDataGenerator.GenerateRegisterRequest();

        // First registration
        await PostAsync("/api/v1/auth/register", request);

        // Act - Try to register again with same email
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = TestDataGenerator.GenerateRegisterRequest(
            password: TestDataGenerator.GenerateInvalidPassword());

        // Act
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = TestDataGenerator.GenerateRegisterRequest(email: "invalid-email");

        // Act
        var response = await PostAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var request = new LoginRequest(email, password);

        // Act
        var response = await PostAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@test.com", "WrongPassword123!");

        // Act
        var response = await PostAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid credentials"));
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();

        // Deactivate the user
        await WithDbContextAsync(async context =>
        {
            var user = await context.Users.FindAsync(userId);
            user!.Deactivate();
            await context.SaveChangesAsync();
        });

        var request = new LoginRequest(email, password);

        // Act
        var response = await PostAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("inactive") || e.Contains("disabled"));
    }

    [Fact]
    public async Task Login_AsAuthAdmin_ReturnsTokensWithoutApplicationId()
    {
        // Arrange
        var (email, password) = GetAuthAdminCredentials();
        var request = new LoginRequest(email, password);

        // Act
        var response = await PostAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var (accessToken, refreshToken) = await GetAuthTokensAsync(email, password);

        var request = new RefreshTokenRequest(refreshToken);

        // Act
        var response = await PostAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.AccessToken.Should().NotBe(accessToken); // Should be a new token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new RefreshTokenRequest("invalid-refresh-token");

        // Act
        var response = await PostAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ReturnsBadRequest()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var (accessToken, refreshToken) = await GetAuthTokensAsync(email, password);

        // Revoke the token by logging out
        SetAuthHeader(accessToken);
        await PostAsync("/api/v1/auth/logout", new LogoutRequest(refreshToken));

        var request = new RefreshTokenRequest(refreshToken);

        // Act
        var response = await PostAsync("/api/v1/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("revoked") || e.Contains("invalid"));
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_RevokesToken()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var (accessToken, refreshToken) = await GetAuthTokensAsync(email, password);

        SetAuthHeader(accessToken);
        var request = new LogoutRequest(refreshToken);

        // Act
        var response = await PostAsync("/api/v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify token is revoked - try to use it
        var refreshResponse = await PostAsync("/api/v1/auth/refresh", new RefreshTokenRequest(refreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithoutSpecificToken_RevokesAllTokens()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var (accessToken, refreshToken) = await GetAuthTokensAsync(email, password);

        SetAuthHeader(accessToken);
        var request = new LogoutRequest(null); // No specific token

        // Act
        var response = await PostAsync("/api/v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all tokens are revoked
        var refreshResponse = await PostAsync("/api/v1/auth/refresh", new RefreshTokenRequest(refreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthHeader();
        var request = new LogoutRequest();

        // Act
        var response = await PostAsync("/api/v1/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var request = new ForgotPasswordRequest(email);

        // Act
        var response = await PostAsync("/api/v1/auth/forgot-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPassword_WithNonexistentEmail_ReturnsSuccessForSecurity()
    {
        // Arrange
        var request = new ForgotPasswordRequest("nonexistent@test.com");

        // Act
        var response = await PostAsync("/api/v1/auth/forgot-password", request);

        // Assert
        // Should return success to prevent email enumeration
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ResetsPassword()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();

        // Request password reset
        await PostAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));

        // Get reset token from database
        var resetToken = await WithDbContextAsync(async context =>
        {
            var user = await context.Users.FindAsync(userId);
            return user!.PasswordResetToken;
        });

        var newPassword = "NewPassword@123";
        var request = new ResetPasswordRequest(email, resetToken!, newPassword);

        // Act
        var response = await PostAsync("/api/v1/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify old password doesn't work
        var oldLoginResponse = await PostAsync("/api/v1/auth/login", new LoginRequest(email, password));
        oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify new password works
        var newLoginResponse = await PostAsync("/api/v1/auth/login", new LoginRequest(email, newPassword));
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ReturnsBadRequest()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();

        // Request password reset
        await PostAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));

        // Get and expire the reset token
        var resetToken = await WithDbContextAsync(async context =>
        {
            var user = await context.Users.FindAsync(userId);
            var token = user!.PasswordResetToken;

            // Set expiry to past
            typeof(User).GetProperty("PasswordResetTokenExpiry")!
                .SetValue(user, DateTime.UtcNow.AddHours(-1));

            await context.SaveChangesAsync();
            return token;
        });

        var request = new ResetPasswordRequest(email, resetToken!, "NewPassword@123");

        // Act
        var response = await PostAsync("/api/v1/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("expired") || e.Contains("invalid"));
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var (userId, email, password) = await CreateRegularUserAsync();
        var request = new ResetPasswordRequest(email, "invalid-token", "NewPassword@123");

        // Act
        var response = await PostAsync("/api/v1/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #endregion
}
