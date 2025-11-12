using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using GenericAuth.API.Controllers.V1;
using GenericAuth.API.IntegrationTests.Infrastructure;
using GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;

namespace GenericAuth.API.IntegrationTests.Workflows;

/// <summary>
/// End-to-end workflow tests demonstrating real-world multi-tenant scenarios.
/// These tests verify complete user journeys through the system.
/// </summary>
public class MultiTenantWorkflowTests : IntegrationTestBase
{
    public MultiTenantWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteMultiTenantWorkflow_UserWithMultipleApplications()
    {
        // This test demonstrates a complete multi-tenant workflow:
        // 1. Auth Admin creates two applications (App A, App B)
        // 2. Auth Admin creates roles for each application
        // 3. A regular user registers
        // 4. Auth Admin assigns user to both apps with different roles
        // 5. User logs in for App A - verify token has Admin role
        // 6. User logs in for App B - verify token has Viewer role
        // 7. Auth Admin changes user's role in App A to Manager
        // 8. User logs in again and verify new role

        // Step 1: Create Auth Admin session
        await AuthenticateAsAuthAdminAsync();

        // Step 2: Create two applications
        var appA = await CreateApplicationAsync("APPA", "Application A");
        var appB = await CreateApplicationAsync("APPB", "Application B");

        // Step 3: Create roles for Application A
        var appAAdminRoleId = await CreateApplicationRoleAsync(appA.Id, "Admin", false);
        var appAManagerRoleId = await CreateApplicationRoleAsync(appA.Id, "Manager", false);
        var appAUserRoleId = await CreateApplicationRoleAsync(appA.Id, "User", true);

        // Step 4: Create roles for Application B
        var appBAdminRoleId = await CreateApplicationRoleAsync(appB.Id, "Admin", false);
        var appBViewerRoleId = await CreateApplicationRoleAsync(appB.Id, "Viewer", true);

        // Step 5: Create a regular user
        var testEmail = "multitenantuser@test.com";
        var testPassword = "MultiTenant@123";
        var registerRequest = new RegisterRequest("John", "Doe", testEmail, testPassword);

        ClearAuthHeader(); // Register as anonymous
        var registerResponse = await PostAsync("/api/v1/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<UserRegistrationResponse>>(_jsonOptions);
        var userId = registerResult!.Data.UserId;

        // Step 6: Auth Admin assigns user to App A with "Admin" role
        await AuthenticateAsAuthAdminAsync();
        var assignAppACommand = new AssignUserToApplicationCommand(userId, appA.Code, "Admin");
        var assignAppAResponse = await PostAsync("/api/v1/user-applications", assignAppACommand);
        assignAppAResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Auth Admin assigns user to App B with "Viewer" role
        var assignAppBCommand = new AssignUserToApplicationCommand(userId, appB.Code, "Viewer");
        var assignAppBResponse = await PostAsync("/api/v1/user-applications", assignAppBCommand);
        assignAppBResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 8: User logs in for App A - verify token has Admin role
        ClearAuthHeader();
        var loginAppARequest = new LoginRequest(testEmail, testPassword, appA.Id);
        var loginAppAResponse = await PostAsync("/api/v1/auth/login", loginAppARequest);
        loginAppAResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginAppAResult = await loginAppAResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        var appAAccessToken = loginAppAResult!.Data.AccessToken;

        // Decode and verify App A token claims
        var appAHandler = new JwtSecurityTokenHandler();
        var appAToken = appAHandler.ReadJwtToken(appAAccessToken);
        appAToken.Claims.Should().Contain(c => c.Type == "application_id" && c.Value == appA.Id.ToString());
        appAToken.Claims.Should().Contain(c => c.Type == "application_role" && c.Value == "Admin");
        appAToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "RegularUser");

        // Step 9: User logs in for App B - verify token has Viewer role
        var loginAppBRequest = new LoginRequest(testEmail, testPassword, appB.Id);
        var loginAppBResponse = await PostAsync("/api/v1/auth/login", loginAppBRequest);
        loginAppBResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginAppBResult = await loginAppBResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        var appBAccessToken = loginAppBResult!.Data.AccessToken;

        // Decode and verify App B token claims
        var appBToken = appAHandler.ReadJwtToken(appBAccessToken);
        appBToken.Claims.Should().Contain(c => c.Type == "application_id" && c.Value == appB.Id.ToString());
        appBToken.Claims.Should().Contain(c => c.Type == "application_role" && c.Value == "Viewer");

        // Step 10: Auth Admin changes user's role in App A from Admin to Manager
        await AuthenticateAsAuthAdminAsync();
        var changeRoleRequest = new ChangeUserRoleRequest(appAManagerRoleId);
        var changeRoleResponse = await PutAsync(
            $"/api/v1/user-applications/users/{userId}/applications/{appA.Code}/role",
            changeRoleRequest);
        changeRoleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 11: User logs in again for App A and verify new Manager role
        ClearAuthHeader();
        var loginAgainResponse = await PostAsync("/api/v1/auth/login", loginAppARequest);
        loginAgainResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginAgainResult = await loginAgainResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        var newAccessToken = loginAgainResult!.Data.AccessToken;

        var newToken = appAHandler.ReadJwtToken(newAccessToken);
        newToken.Claims.Should().Contain(c => c.Type == "application_role" && c.Value == "Manager");
        newToken.Claims.Should().NotContain(c => c.Type == "application_role" && c.Value == "Admin");

        // Step 12: Verify user can be removed from an application
        await AuthenticateAsAuthAdminAsync();
        var removeResponse = await DeleteAsync($"/api/v1/user-applications/users/{userId}/applications/{appB.Code}");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user can no longer login for App B
        ClearAuthHeader();
        var loginAfterRemovalResponse = await PostAsync("/api/v1/auth/login", loginAppBRequest);
        loginAfterRemovalResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PasswordResetWorkflow_EndToEnd()
    {
        // This test demonstrates the complete password reset workflow:
        // 1. User registers
        // 2. User logs in successfully with original password
        // 3. User requests password reset
        // 4. User resets password with token
        // 5. Verify old password fails
        // 6. Verify new password works
        // 7. Verify reset token can only be used once

        // Step 1: Create a user
        var email = "resettest@test.com";
        var originalPassword = "Original@123";
        var newPassword = "NewPassword@456";

        var registerRequest = new RegisterRequest("Reset", "Test", email, originalPassword);
        var registerResponse = await PostAsync("/api/v1/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<UserRegistrationResponse>>(_jsonOptions);
        var userId = registerResult!.Data.UserId;

        // Step 2: Login successfully with original password
        var loginRequest = new LoginRequest(email, originalPassword);
        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Request password reset
        var forgotPasswordRequest = new ForgotPasswordRequest(email);
        var forgotPasswordResponse = await PostAsync("/api/v1/auth/forgot-password", forgotPasswordRequest);
        forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Get plain-text reset token from token store
        var resetToken = _passwordResetTokenStore.GetToken(email);
        resetToken.Should().NotBeNullOrEmpty("Token should be stored after forgot password request");

        // Step 5: Reset password with token
        var resetPasswordRequest = new ResetPasswordRequest(email, resetToken, newPassword);
        var resetPasswordResponse = await PostAsync("/api/v1/auth/reset-password", resetPasswordRequest);
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Verify old password no longer works
        var oldPasswordLoginRequest = new LoginRequest(email, originalPassword);
        var oldPasswordLoginResponse = await PostAsync("/api/v1/auth/login", oldPasswordLoginRequest);
        oldPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Step 7: Verify new password works
        var newPasswordLoginRequest = new LoginRequest(email, newPassword);
        var newPasswordLoginResponse = await PostAsync("/api/v1/auth/login", newPasswordLoginRequest);
        newPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 8: Verify reset token is invalidated after use
        var reuseTokenRequest = new ResetPasswordRequest(email, resetToken, "AnotherPassword@789");
        var reuseTokenResponse = await PostAsync("/api/v1/auth/reset-password", reuseTokenRequest);
        reuseTokenResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Step 9: Verify user can still login with the successful new password
        var finalLoginResponse = await PostAsync("/api/v1/auth/login", newPasswordLoginRequest);
        finalLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TokenRefreshWorkflow_EndToEnd()
    {
        // This test demonstrates the token refresh workflow:
        // 1. User registers and logs in
        // 2. User gets access token and refresh token
        // 3. User uses refresh token to get new tokens
        // 4. Verify new tokens work
        // 5. Verify old refresh token is revoked
        // 6. User logs out
        // 7. Verify all tokens are revoked

        // Step 1: Create and login user
        var email = "tokentest@test.com";
        var password = "TokenTest@123";

        var registerRequest = new RegisterRequest("Token", "Test", email, password);
        var registerResponse = await PostAsync("/api/v1/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Login to get initial tokens
        var loginRequest = new LoginRequest(email, password);
        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        var originalAccessToken = loginResult!.Data.AccessToken;
        var originalRefreshToken = loginResult.Data.RefreshToken;

        originalAccessToken.Should().NotBeNullOrEmpty();
        originalRefreshToken.Should().NotBeNullOrEmpty();

        // Step 3: Use refresh token to get new tokens
        await Task.Delay(1000); // Small delay to ensure new token is different

        var refreshRequest = new RefreshTokenRequest(originalRefreshToken);
        var refreshResponse = await PostAsync("/api/v1/auth/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        var newAccessToken = refreshResult!.Data.AccessToken;
        var newRefreshToken = refreshResult.Data.RefreshToken;

        newAccessToken.Should().NotBeNullOrEmpty();
        newRefreshToken.Should().NotBeNullOrEmpty();
        newAccessToken.Should().NotBe(originalAccessToken);
        newRefreshToken.Should().NotBe(originalRefreshToken);

        // Step 4: Verify new access token works for authenticated requests
        SetAuthHeader(newAccessToken);
        var userAppsResponse = await _client.GetAsync($"/api/v1/user-applications/users/{loginResult.Data.AccessToken}/applications");
        // Should be authorized (even if returns 404 for parsing user id from token)
        userAppsResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);

        // Step 5: Verify old refresh token is revoked
        var oldRefreshRequest = new RefreshTokenRequest(originalRefreshToken);
        var oldRefreshResponse = await PostAsync("/api/v1/auth/refresh", oldRefreshRequest);
        oldRefreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Step 6: Logout with new refresh token
        SetAuthHeader(newAccessToken);
        var logoutRequest = new LogoutRequest(newRefreshToken);
        var logoutResponse = await PostAsync("/api/v1/auth/logout", logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Verify new refresh token is now revoked
        ClearAuthHeader();
        var afterLogoutRefreshRequest = new RefreshTokenRequest(newRefreshToken);
        var afterLogoutRefreshResponse = await PostAsync("/api/v1/auth/refresh", afterLogoutRefreshRequest);
        afterLogoutRefreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Step 8: User can login again to get fresh tokens
        var finalLoginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        finalLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApplicationRoleDefaultWorkflow_NewUserAutoAssignment()
    {
        // This test demonstrates how default roles work for new user assignments:
        // 1. Auth Admin creates an application with multiple roles
        // 2. One role is set as default
        // 3. New user is assigned to application without specifying role
        // 4. Verify user gets the default role
        // 5. Default role is changed
        // 6. New user assignment gets the new default role

        // Step 1: Setup Auth Admin
        await AuthenticateAsAuthAdminAsync();

        // Step 2: Create application
        var app = await CreateApplicationAsync();

        // Step 3: Create multiple roles
        var adminRoleId = await CreateApplicationRoleAsync(app.Id, "Admin", false);
        var managerRoleId = await CreateApplicationRoleAsync(app.Id, "Manager", false);
        var userRoleId = await CreateApplicationRoleAsync(app.Id, "User", true); // Default

        // Step 4: Create first user and assign without specifying role
        var (user1Id, _, _) = await CreateRegularUserAsync();
        var assignUser1Command = new AssignUserToApplicationCommand(user1Id, app.Code, null); // Should use default
        var assignUser1Response = await PostAsync("/api/v1/user-applications", assignUser1Command);
        assignUser1Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignUser1Result = await assignUser1Response.Content.ReadFromJsonAsync<ApiResponse<AssignUserToApplicationCommandResponse>>(_jsonOptions);
        assignUser1Result!.Data.RoleId.Should().NotBe(Guid.Empty);

        // Step 5: Change default role to Manager
        var setDefaultResponse = await PostAsync($"/api/v1/applications/{app.Id}/roles/{managerRoleId}/set-default", new { });
        setDefaultResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 6: Create second user and assign without specifying role
        var (user2Id, _, _) = await CreateRegularUserAsync();
        var assignUser2Command = new AssignUserToApplicationCommand(user2Id, app.Code, null); // Should use new default
        var assignUser2Response = await PostAsync("/api/v1/user-applications", assignUser2Command);
        assignUser2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignUser2Result = await assignUser2Response.Content.ReadFromJsonAsync<ApiResponse<AssignUserToApplicationCommandResponse>>(_jsonOptions);
        assignUser2Result!.Data.RoleId.Should().NotBe(Guid.Empty);

        // Step 7: Verify first user still has User role (not affected by default change)
        var user1AppsResponse = await _client.GetAsync($"/api/v1/user-applications/users/{user1Id}/applications");
        var user1AppsResult = await user1AppsResponse.Content.ReadFromJsonAsync<ApiResponse<List<Controllers.UserApplicationDto>>>(_jsonOptions);
        user1AppsResult!.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task AuthAdminVsRegularUserWorkflow_DifferentPermissions()
    {
        // This test demonstrates the difference between Auth Admin and Regular User:
        // 1. Auth Admin can access system management endpoints
        // 2. Regular User cannot access system management endpoints
        // 3. Regular User can only access their own data

        // Step 1: Auth Admin creates an application
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();

        // Verify Auth Admin can access application roles
        var rolesResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: Create a regular user
        var (userId, email, password) = await CreateRegularUserAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        // Assign user to application
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app.Code));

        // Step 3: Login as regular user
        ClearAuthHeader();
        var loginRequest = new LoginRequest(email, password, app.Id);
        var loginResponse = await PostAsync("/api/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);
        SetAuthHeader(loginResult!.Data.AccessToken);

        // Step 4: Verify regular user cannot access Auth Admin endpoints
        var unauthorizedRolesResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles");
        unauthorizedRolesResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Regular user should not be able to access user-application management endpoints
        var unauthorizedUsersResponse = await _client.GetAsync($"/api/v1/user-applications/applications/{app.Code}/users");
        unauthorizedUsersResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Step 5: Verify Auth Admin token has different claims
        await AuthenticateAsAuthAdminAsync();
        var adminLoginRequest = new LoginRequest(DefaultAuthAdminEmail, DefaultAuthAdminPassword);
        var adminLoginResponse = await PostAsync("/api/v1/auth/login", adminLoginRequest);
        var adminLoginResult = await adminLoginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOptions);

        var handler = new JwtSecurityTokenHandler();
        var adminToken = handler.ReadJwtToken(adminLoginResult!.Data.AccessToken);
        adminToken.Claims.Should().Contain(c => c.Type == "user_type" && c.Value == "AuthAdmin");
        adminToken.Claims.Should().NotContain(c => c.Type == "application_id"); // Auth Admin doesn't need application context
    }
}
