using Asp.Versioning;
using GenericAuth.Application.Features.ApplicationRoles.Commands.ActivateApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.AddPermissionToApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.CreateApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.DeactivateApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.DeleteApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.RemovePermissionFromApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.SetDefaultApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Commands.UpdateApplicationRole;
using GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoleById;
using GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers.V1;

/// <summary>
/// Controller for managing application-specific roles.
/// Only accessible by Auth Admin users.
/// Application roles are the primary multi-tenant mechanism - each application has its own isolated set of roles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/applications/{appId:guid}/roles")]
[Authorize(Policy = "AuthAdminOnly")]
[Produces("application/json")]
public class ApplicationRolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationRolesController> _logger;

    public ApplicationRolesController(IMediator mediator, ILogger<ApplicationRolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of roles for a specific application.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by name or description</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of application roles</returns>
    /// <response code="200">Roles retrieved successfully</response>
    /// <response code="404">Application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApplicationRoles(
        Guid appId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        _logger.LogInformation(
            "Getting roles for application {ApplicationId} - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, IsActive: {IsActive}",
            appId, pageNumber, pageSize, searchTerm, isActive);

        var query = new GetApplicationRolesQuery(appId, pageNumber, pageSize, searchTerm, isActive);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to get roles for application {ApplicationId}: {Errors}",
                appId, string.Join(", ", result.Errors));
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new
        {
            success = true,
            data = result.Value.Items,
            pagination = new
            {
                pageNumber = result.Value.PageNumber,
                pageSize = pageSize,
                totalPages = result.Value.TotalPages,
                totalCount = result.Value.TotalCount,
                hasPreviousPage = result.Value.HasPreviousPage,
                hasNextPage = result.Value.HasNextPage
            }
        });
    }

    /// <summary>
    /// Gets detailed information about a specific application role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Role details including permissions and user count</returns>
    /// <response code="200">Role found</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApplicationRoleById(Guid appId, Guid roleId)
    {
        _logger.LogInformation("Getting role {RoleId} for application {ApplicationId}", roleId, appId);

        var query = new GetApplicationRoleByIdQuery(appId, roleId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Role {RoleId} not found for application {ApplicationId}", roleId, appId);
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Creates a new role for an application.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="command">Role creation details</param>
    /// <returns>Created role information</returns>
    /// <response code="200">Role created successfully</response>
    /// <response code="400">Invalid request or role name already exists</response>
    /// <response code="404">Application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateApplicationRole(
        Guid appId,
        [FromBody] CreateApplicationRoleRequest request)
    {
        _logger.LogInformation("Creating role '{RoleName}' for application {ApplicationId}", request.Name, appId);

        var command = new CreateApplicationRoleCommand(
            appId,
            request.Name,
            request.Description,
            request.IsDefault);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create role for application {ApplicationId}: {Errors}",
                appId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role '{RoleName}' created successfully for application {ApplicationId}",
            request.Name, appId);

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Updates an existing application role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="request">Updated role information</param>
    /// <returns>Updated role information</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateApplicationRole(
        Guid appId,
        Guid roleId,
        [FromBody] UpdateApplicationRoleRequest request)
    {
        _logger.LogInformation("Updating role {RoleId} for application {ApplicationId}", roleId, appId);

        var command = new UpdateApplicationRoleCommand(appId, roleId, request.Name, request.Description);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update role {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} updated successfully", roleId);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Deletes an application role.
    /// Cannot delete default roles or roles with user assignments.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Role deleted successfully</response>
    /// <response code="400">Cannot delete role (default or has user assignments)</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteApplicationRole(Guid appId, Guid roleId)
    {
        _logger.LogInformation("Deleting role {RoleId} from application {ApplicationId}", roleId, appId);

        var command = new DeleteApplicationRoleCommand(appId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete role {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} deleted successfully", roleId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Activates an application role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Activation confirmation</returns>
    /// <response code="200">Role activated successfully</response>
    /// <response code="400">Role is already active</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{roleId:guid}/activate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivateApplicationRole(Guid appId, Guid roleId)
    {
        _logger.LogInformation("Activating role {RoleId} for application {ApplicationId}", roleId, appId);

        var command = new ActivateApplicationRoleCommand(appId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate role {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} activated successfully", roleId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Deactivates an application role.
    /// Cannot deactivate the default role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Deactivation confirmation</returns>
    /// <response code="200">Role deactivated successfully</response>
    /// <response code="400">Role is already inactive or is the default role</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{roleId:guid}/deactivate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateApplicationRole(Guid appId, Guid roleId)
    {
        _logger.LogInformation("Deactivating role {RoleId} for application {ApplicationId}", roleId, appId);

        var command = new DeactivateApplicationRoleCommand(appId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate role {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} deactivated successfully", roleId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Sets a role as the default role for the application.
    /// This will remove the default status from any other role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Role set as default successfully</response>
    /// <response code="400">Role is inactive or already default</response>
    /// <response code="404">Application or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{roleId:guid}/set-default")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetDefaultApplicationRole(Guid appId, Guid roleId)
    {
        _logger.LogInformation("Setting role {RoleId} as default for application {ApplicationId}", roleId, appId);

        var command = new SetDefaultApplicationRoleCommand(appId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to set role {RoleId} as default: {Errors}",
                roleId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} set as default successfully", roleId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Adds a permission to an application role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Permission added successfully</response>
    /// <response code="400">Permission already assigned or invalid</response>
    /// <response code="404">Application, role, or permission not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddPermissionToRole(Guid appId, Guid roleId, Guid permissionId)
    {
        _logger.LogInformation(
            "Adding permission {PermissionId} to role {RoleId} in application {ApplicationId}",
            permissionId, roleId, appId);

        var command = new AddPermissionToApplicationRoleCommand(appId, roleId, permissionId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to add permission: {Errors}", string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Permission {PermissionId} added to role {RoleId} successfully", permissionId, roleId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Removes a permission from an application role.
    /// </summary>
    /// <param name="appId">Application ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Permission removed successfully</response>
    /// <response code="400">Permission not assigned to role</response>
    /// <response code="404">Application, role, or permission not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemovePermissionFromRole(Guid appId, Guid roleId, Guid permissionId)
    {
        _logger.LogInformation(
            "Removing permission {PermissionId} from role {RoleId} in application {ApplicationId}",
            permissionId, roleId, appId);

        var command = new RemovePermissionFromApplicationRoleCommand(appId, roleId, permissionId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove permission: {Errors}", string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Permission {PermissionId} removed from role {RoleId} successfully", permissionId, roleId);
        return Ok(new { success = true, message = result.Value });
    }
}

/// <summary>
/// Request model for creating an application role.
/// </summary>
public record CreateApplicationRoleRequest(
    string Name,
    string Description,
    bool IsDefault = false);

/// <summary>
/// Request model for updating an application role.
/// </summary>
public record UpdateApplicationRoleRequest(
    string Name,
    string Description);
