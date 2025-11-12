using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericAuth.API.IntegrationTests.Helpers;
using GenericAuth.API.IntegrationTests.Infrastructure;
using GenericAuth.Application.Features.Roles.Queries;
using GenericAuth.Domain.Entities;

namespace GenericAuth.API.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for RolesController (System Roles).
/// Tests system role management for Auth Admin users.
/// </summary>
public class RolesControllerTests : IntegrationTestBase
{
    public RolesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Get All Roles Tests

    [Fact]
    public async Task GetAllRoles_ReturnsPaginatedRoles()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();

        // Create system roles
        await CreateSystemRoleAsync("SystemAdmin");
        await CreateSystemRoleAsync("SystemManager");

        // Act
        var response = await _client.GetAsync("/api/v1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllRoles_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();

        await CreateSystemRoleAsync("Administrator");
        await CreateSystemRoleAsync("Manager");
        await CreateSystemRoleAsync("Admin Assistant");

        // Act
        var response = await _client.GetAsync("/api/v1/roles?searchTerm=admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Data.Should().OnlyContain(r => r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAllRoles_WithActiveFilter_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();

        var activeRoleId = await CreateSystemRoleAsync("Active Role");
        var inactiveRoleId = await CreateSystemRoleAsync("Inactive Role");

        // Deactivate one role
        await WithDbContextAsync(async context =>
        {
            var role = await context.Roles.FindAsync(inactiveRoleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act
        var response = await _client.GetAsync("/api/v1/roles?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Data.Should().NotContain(r => r.Name == "Inactive Role");
    }

    [Fact]
    public async Task GetAllRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/v1/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Role By Id Tests

    [Fact]
    public async Task GetRoleById_WithValidId_ReturnsRoleDetails()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var roleId = await CreateSystemRoleAsync("Test Role");

        // Act
        var response = await _client.GetAsync($"/api/v1/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(roleId);
        result.Data.Name.Should().Be("Test Role");
    }

    [Fact]
    public async Task GetRoleById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/roles/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Create Role Tests

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsCreatedRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var request = new CreateRoleRequest("SystemAuditor", "Audits system activities");

        // Act
        var response = await PostAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Name.Should().Be("SystemAuditor");
        result.Data.Description.Should().Be("Audits system activities");
        result.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        await CreateSystemRoleAsync("DuplicateRole");

        var request = new CreateRoleRequest("DuplicateRole", "Attempt duplicate");

        // Act
        var response = await PostAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
    }

    [Fact]
    public async Task CreateRole_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var request = new CreateRoleRequest("", "Empty name is invalid");

        // Act
        var response = await PostAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Role Tests

    [Fact]
    public async Task UpdateRole_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var roleId = await CreateSystemRoleAsync("Original Name");

        var request = new UpdateRoleRequest("Updated Name", "Updated description");

        // Act
        var response = await PutAsync($"/api/v1/roles/{roleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Data.Name.Should().Be("Updated Name");
        result.Data.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateRole_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        await CreateSystemRoleAsync("Existing Role");
        var roleId = await CreateSystemRoleAsync("Role to Update");

        var request = new UpdateRoleRequest("Existing Role", "Try duplicate");

        // Act
        var response = await PutAsync($"/api/v1/roles/{roleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Delete Role Tests

    [Fact]
    public async Task DeleteRole_WithoutUsers_DeactivatesRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var roleId = await CreateSystemRoleAsync("Role to Delete");

        // Act
        var response = await DeleteAsync($"/api/v1/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role still exists but is deactivated
        var verifyResponse = await _client.GetAsync($"/api/v1/roles/{roleId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeFalse();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task ActivateRole_ActivatesDeactivatedRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var roleId = await CreateSystemRoleAsync("Role to Activate");

        // Deactivate first
        await WithDbContextAsync(async context =>
        {
            var role = await context.Roles.FindAsync(roleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act
        var response = await PostAsync($"/api/v1/roles/{roleId}/activate", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify
        var verifyResponse = await _client.GetAsync($"/api/v1/roles/{roleId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateRole_DeactivatesActiveRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var roleId = await CreateSystemRoleAsync("Role to Deactivate");

        // Act
        var response = await PostAsync($"/api/v1/roles/{roleId}/deactivate", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify
        var verifyResponse = await _client.GetAsync($"/api/v1/roles/{roleId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<RoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateSystemRoleAsync(string? name = null)
    {
        return await WithDbContextAsync(async context =>
        {
            // Generate unique role name with timestamp suffix to prevent duplicates
            // If name is provided explicitly, use it as-is (test may intentionally want duplicates)
            var roleName = name ?? $"SystemRole_{Guid.NewGuid().ToString().Substring(0, 8)}";

            var role = Role.Create(roleName, $"Description for {roleName}");
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();
            return role.Id;
        });
    }

    #endregion
}

#region Request Models

public record CreateRoleRequest(string Name, string Description);
public record UpdateRoleRequest(string Name, string Description);

#endregion
