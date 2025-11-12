using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GenericAuth.API.Controllers.V1;
using GenericAuth.API.IntegrationTests.Helpers;
using GenericAuth.API.IntegrationTests.Infrastructure;
using GenericAuth.Application.Features.ApplicationRoles.Queries;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.API.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ApplicationRolesController endpoints.
/// Tests application role management including CRUD operations, permissions, and default role handling.
/// </summary>
public class ApplicationRolesControllerTests : IntegrationTestBase
{
    public ApplicationRolesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Get Roles Tests

    [Fact]
    public async Task GetRoles_ReturnsAllRolesForApplication()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();

        // Create multiple roles
        await CreateApplicationRoleAsync(app.Id, "Admin", false);
        await CreateApplicationRoleAsync(app.Id, "User", true);
        await CreateApplicationRoleAsync(app.Id, "Manager", false);

        // Act
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Pagination.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetRoles_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();

        // Create 5 roles
        for (int i = 1; i <= 5; i++)
        {
            await CreateApplicationRoleAsync(app.Id, $"Role{i}", false);
        }

        // Act - Get page 2 with page size 2
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles?pageNumber=2&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Pagination.PageNumber.Should().Be(2);
        result.Pagination.TotalPages.Should().Be(3);
        result.Pagination.HasPreviousPage.Should().BeTrue();
        result.Pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetRoles_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();

        await CreateApplicationRoleAsync(app.Id, "Administrator", false);
        await CreateApplicationRoleAsync(app.Id, "User", false);
        await CreateApplicationRoleAsync(app.Id, "Admin Assistant", false);

        // Act - Search for "admin"
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles?searchTerm=admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(r => r.Name.Contains("Admin", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetRoles_WithActiveFilter_ReturnsOnlyActive()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();

        var activeRoleId = await CreateApplicationRoleAsync(app.Id, "Active Role", false);
        var inactiveRoleId = await CreateApplicationRoleAsync(app.Id, "Inactive Role", false);

        // Deactivate one role
        await WithDbContextAsync(async context =>
        {
            var role = await context.ApplicationRoles.FindAsync(inactiveRoleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act - Filter for active roles
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles?isActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.First().Name.Should().Be("Active Role");
    }

    [Fact]
    public async Task GetRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthHeader();
        var app = await CreateApplicationAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Role By Id Tests

    [Fact]
    public async Task GetRoleById_WithValidId_ReturnsRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Test Role", false);

        // Act
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
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
        var app = await CreateApplicationAsync();
        var invalidRoleId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{invalidRoleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleById_IncludesDetailsAndPermissions()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Test Role", false);

        // Act
        var response = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDetailDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(roleId);
        result.Data.Permissions.Should().NotBeNull();
        result.Data.UserCount.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Create Role Tests

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsCreatedRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var request = new CreateApplicationRoleRequest("Manager", "Manages the team", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Name.Should().Be("Manager");
        result.Data.Description.Should().Be("Manages the team");
        result.Data.IsDefault.Should().BeFalse();
        result.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "Admin", false);

        var request = new CreateApplicationRoleRequest("Admin", "Duplicate name", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles", request);

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
        var app = await CreateApplicationAsync();
        var request = new CreateApplicationRoleRequest("", "Invalid - empty name", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRole_AsDefault_SetsDefaultFlag()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var request = new CreateApplicationRoleRequest("Default User", "Default role for users", true);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Data.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRole_WithNonexistentApplication_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var invalidAppId = Guid.NewGuid();
        var request = new CreateApplicationRoleRequest("Test Role", "Test", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{invalidAppId}/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Update Role Tests

    [Fact]
    public async Task UpdateRole_WithValidData_ReturnsUpdatedRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Original Name", false);

        var request = new UpdateApplicationRoleRequest("Updated Name", "Updated description");

        // Act
        var response = await PutAsync($"/api/v1/applications/{app.Id}/roles/{roleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Name.Should().Be("Updated Name");
        result.Data.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateRole_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        await CreateApplicationRoleAsync(app.Id, "Existing Role", false);
        var roleId = await CreateApplicationRoleAsync(app.Id, "Role to Update", false);

        var request = new UpdateApplicationRoleRequest("Existing Role", "Try to use existing name");

        // Act
        var response = await PutAsync($"/api/v1/applications/{app.Id}/roles/{roleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRole_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var invalidRoleId = Guid.NewGuid();
        var request = new UpdateApplicationRoleRequest("Updated Name", "Description");

        // Act
        var response = await PutAsync($"/api/v1/applications/{app.Id}/roles/{invalidRoleId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete Role Tests

    [Fact]
    public async Task DeleteRole_WithoutUsers_SuccessfullyDeletes()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Role to Delete", false);

        // Act
        var response = await DeleteAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role is deleted
        var verifyResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_DefaultRole_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Default Role", true);

        // Act
        var response = await DeleteAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("default"));
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task ActivateRole_ActivatesInactiveRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Role to Activate", false);

        // Deactivate first
        await WithDbContextAsync(async context =>
        {
            var role = await context.ApplicationRoles.FindAsync(roleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles/{roleId}/activate", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role is active
        var verifyResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateRole_DeactivatesActiveRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Role to Deactivate", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles/{roleId}/deactivate", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role is inactive
        var verifyResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateRole_DefaultRole_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Default Role", true);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles/{roleId}/deactivate", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #endregion

    #region Set Default Role Tests

    [Fact]
    public async Task SetDefaultRole_RemovesPreviousDefault()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var oldDefaultRoleId = await CreateApplicationRoleAsync(app.Id, "Old Default", true);
        var newDefaultRoleId = await CreateApplicationRoleAsync(app.Id, "New Default", false);

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles/{newDefaultRoleId}/set-default", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify old default is no longer default
        var oldRoleResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{oldDefaultRoleId}");
        var oldRoleResult = await oldRoleResponse.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        oldRoleResult!.Data.IsDefault.Should().BeFalse();

        // Verify new role is default
        var newRoleResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{newDefaultRoleId}");
        var newRoleResult = await newRoleResponse.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        newRoleResult!.Data.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task SetDefaultRole_ActivatesInactiveRole()
    {
        // Arrange
        await AuthenticateAsAuthAdminAsync();
        var app = await CreateApplicationAsync();
        var roleId = await CreateApplicationRoleAsync(app.Id, "Inactive Role", false);

        // Deactivate role
        await WithDbContextAsync(async context =>
        {
            var role = await context.ApplicationRoles.FindAsync(roleId);
            role!.Deactivate();
            await context.SaveChangesAsync();
        });

        // Act
        var response = await PostAsync($"/api/v1/applications/{app.Id}/roles/{roleId}/set-default", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role is now active and default
        var verifyResponse = await _client.GetAsync($"/api/v1/applications/{app.Id}/roles/{roleId}");
        var verifyResult = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse<ApplicationRoleDto>>(_jsonOptions);
        verifyResult!.Data.IsActive.Should().BeTrue();
        verifyResult.Data.IsDefault.Should().BeTrue();
    }

    #endregion
}

#region Response Models

public class PaginatedApiResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

#endregion
