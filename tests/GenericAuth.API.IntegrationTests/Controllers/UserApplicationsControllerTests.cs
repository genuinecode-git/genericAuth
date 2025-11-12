using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericAuth.API.IntegrationTests.Infrastructure;
using GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.API.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for UserApplicationsController.
/// Tests user-to-application assignment and role management.
/// </summary>
public class UserApplicationsControllerTests : IntegrationTestBase
{
    public UserApplicationsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Assign User Tests

    [Fact]
    public async Task AssignUser_WithSpecificRole_AssignsSuccessfully()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Manager", false);

        var command = new AssignUserToApplicationCommand(userId, app.Code, "Manager");

        // Act
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AssignUserToApplicationCommandResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(userId);
        result.Data.ApplicationId.Should().Be(app.Id);
        result.Data.RoleId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AssignUser_WithoutRole_UsesDefaultRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "DefaultUser", true);

        var command = new AssignUserToApplicationCommand(userId, app.Code, null); // Use default

        // Act
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AssignUserToApplicationCommandResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.RoleId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task AssignUser_NoDefaultRole_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "NonDefaultRole", false);

        var command = new AssignUserToApplicationCommand(userId, app.Code, null); // No default role exists

        // Act
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("default role"));
    }

    [Fact]
    public async Task AssignUser_AlreadyAssigned_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        var command = new AssignUserToApplicationCommand(userId, app.Code);

        // Assign first time
        await PostAsync("/api/v1/user-applications", command);

        // Act - Try to assign again
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already assigned"));
    }

    [Fact]
    public async Task AssignUser_WithNonexistentUser_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        var command = new AssignUserToApplicationCommand(Guid.NewGuid(), app.Code);

        // Act
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignUser_WithNonexistentApplication_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();

        var command = new AssignUserToApplicationCommand(userId, "NONEXIST");

        // Act
        var response = await PostAsync("/api/v1/user-applications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get User Applications Tests

    [Fact]
    public async Task GetUserApplications_ReturnsAllUserApps()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app1 = await CreateApplicationAsync();
        var app2 = await CreateApplicationAsync();

        await CreateApplicationRoleAsync(app1.Id, "User", true);
        await CreateApplicationRoleAsync(app2.Id, "Admin", true);

        // Assign to both apps
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app1.Code));
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app2.Code));

        // Act
        var response = await _client.GetAsync($"/api/v1/user-applications/users/{userId}/applications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserApplicationDto>>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserApplications_ForUserWithNoApps_ReturnsEmpty()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/user-applications/users/{userId}/applications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserApplicationDto>>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserApplications_WithInvalidUser_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var invalidUserId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/user-applications/users/{invalidUserId}/applications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Application Users Tests

    [Fact]
    public async Task GetApplicationUsers_ReturnsPaginatedUsers()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        // Create and assign multiple users
        for (int i = 0; i < 3; i++)
        {
            var (userId, _, _) = await CreateRegularUserAsync();
            await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app.Code));
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/user-applications/applications/{app.Code}/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationUserDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Pagination.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetApplicationUsers_WithSearch_ReturnsFilteredUsers()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        var (userId1, email1, _) = await CreateRegularUserAsync("john.doe@test.com");
        var (userId2, email2, _) = await CreateRegularUserAsync("jane.smith@test.com");

        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId1, app.Code));
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId2, app.Code));

        // Act
        var response = await _client.GetAsync($"/api/v1/user-applications/applications/{app.Code}/users?searchTerm=john");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationUserDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.Data.First().Email.Should().Contain("john");
    }

    [Fact]
    public async Task GetApplicationUsers_WithNonexistentApp_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/user-applications/applications/NONEXIST/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Change User Role Tests

    [Fact]
    public async Task ChangeUserRole_WithValidRole_UpdatesSuccessfully()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        var userRoleId = await CreateApplicationRoleAsync(app.Id, "User", true);
        var adminRoleId = await CreateApplicationRoleAsync(app.Id, "Admin", false);

        // Assign user with User role
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app.Code));

        var request = new ChangeUserRoleRequest(adminRoleId);

        // Act
        var response = await PutAsync($"/api/v1/user-applications/users/{userId}/applications/{app.Code}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role changed
        var verifyResponse = await _client.GetAsync($"/api/v1/user-applications/users/{userId}/applications");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<List<UserApplicationDto>>>(_jsonOptions);
        verifyResult!.Data.First().RoleName.Should().Be("Admin");
    }

    [Fact]
    public async Task ChangeUserRole_WithInactiveRole_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);
        var inactiveRoleId = await CreateApplicationRoleAsync(app.Id, "Inactive", false);

        // Deactivate role
        await WithDbContextAsync(async context =>
        {
            var role = await context.ApplicationRoles.FindAsync(inactiveRoleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Assign user
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app.Code));

        var request = new ChangeUserRoleRequest(inactiveRoleId);

        // Act
        var response = await PutAsync($"/api/v1/user-applications/users/{userId}/applications/{app.Code}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeUserRole_RoleFromDifferentApp_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app1 = await CreateApplicationAsync();
        var app2 = await CreateApplicationAsync();

        await CreateApplicationRoleAsync(app1.Id, "User", true);
        var app2RoleId = await CreateApplicationRoleAsync(app2.Id, "Admin", true);

        // Assign to app1
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app1.Code));

        var request = new ChangeUserRoleRequest(app2RoleId);

        // Act - Try to change to role from app2
        var response = await PutAsync($"/api/v1/user-applications/users/{userId}/applications/{app1.Code}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Remove User Tests

    [Fact]
    public async Task RemoveUser_FromApplication_RemovesSuccessfully()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "User", true);

        // Assign user
        await PostAsync("/api/v1/user-applications", new AssignUserToApplicationCommand(userId, app.Code));

        // Act
        var response = await DeleteAsync($"/api/v1/user-applications/users/{userId}/applications/{app.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user is removed
        var verifyResponse = await _client.GetAsync($"/api/v1/user-applications/users/{userId}/applications");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<List<UserApplicationDto>>>(_jsonOptions);
        verifyResult!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveUser_NotAssigned_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var (userId, _, _) = await CreateRegularUserAsync();
        var app = await CreateApplicationAsync();

        // Act - Try to remove user that was never assigned
        var response = await DeleteAsync($"/api/v1/user-applications/users/{userId}/applications/{app.Code}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}

#region Response Models

public record ChangeUserRoleRequest(Guid NewApplicationRoleId);

public class UserApplicationDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class ApplicationUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

#endregion
