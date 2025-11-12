using Asp.Versioning;
using GenericAuth.Application.Features.Roles.Commands.ActivateRole;
using GenericAuth.Application.Features.Roles.Commands.AddPermissionToRole;
using GenericAuth.Application.Features.Roles.Commands.CreateRole;
using GenericAuth.Application.Features.Roles.Commands.DeactivateRole;
using GenericAuth.Application.Features.Roles.Commands.DeleteRole;
using GenericAuth.Application.Features.Roles.Commands.RemovePermissionFromRole;
using GenericAuth.Application.Features.Roles.Commands.UpdateRole;
using GenericAuth.Application.Features.Roles.Queries.GetAllRoles;
using GenericAuth.Application.Features.Roles.Queries.GetRoleById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers.V1;

/// <summary>
/// Controller for managing system roles.
/// Only accessible by Auth Admin users.
/// System roles are for Auth Admin users only and are separate from application roles.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/roles")]
[Authorize(Policy = "AuthAdminOnly")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of system roles.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by name or description</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of system roles</returns>
    /// <response code="200">Roles retrieved successfully</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRoles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        _logger.LogInformation(
            "Getting system roles - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, IsActive: {IsActive}",
            pageNumber, pageSize, searchTerm, isActive);

        var query = new GetAllRolesQuery(pageNumber, pageSize, searchTerm, isActive);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to get system roles: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
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
    /// Gets detailed information about a specific system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details including permissions and assigned users</returns>
    /// <response code="200">Role found</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        _logger.LogInformation("Getting system role {RoleId}", id);

        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("System role {RoleId} not found", id);
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Creates a new system role.
    /// </summary>
    /// <param name="request">Role creation details</param>
    /// <returns>Created role information</returns>
    /// <response code="200">Role created successfully</response>
    /// <response code="400">Invalid request or role name already exists</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        _logger.LogInformation("Creating system role '{RoleName}'", request.Name);

        var command = new CreateRoleCommand(request.Name, request.Description);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create system role: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("System role '{RoleName}' created successfully", request.Name);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Updates an existing system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Updated role information</param>
    /// <returns>Updated role information</returns>
    /// <response code="200">Role updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        _logger.LogInformation("Updating system role {RoleId}", id);

        var command = new UpdateRoleCommand(id, request.Name, request.Description);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update system role {RoleId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("System role {RoleId} updated successfully", id);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Deletes a system role.
    /// Cannot delete roles with user assignments.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Role deleted successfully</response>
    /// <response code="400">Cannot delete role (has user assignments)</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        _logger.LogInformation("Deleting system role {RoleId}", id);

        var command = new DeleteRoleCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to delete system role {RoleId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("System role {RoleId} deleted successfully", id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Activates a system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Activation confirmation</returns>
    /// <response code="200">Role activated successfully</response>
    /// <response code="400">Role is already active</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivateRole(Guid id)
    {
        _logger.LogInformation("Activating system role {RoleId}", id);

        var command = new ActivateRoleCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate system role {RoleId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("System role {RoleId} activated successfully", id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Deactivates a system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Deactivation confirmation</returns>
    /// <response code="200">Role deactivated successfully</response>
    /// <response code="400">Role is already inactive</response>
    /// <response code="404">Role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateRole(Guid id)
    {
        _logger.LogInformation("Deactivating system role {RoleId}", id);

        var command = new DeactivateRoleCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate system role {RoleId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("System role {RoleId} deactivated successfully", id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Adds a permission to a system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Permission added successfully</response>
    /// <response code="400">Permission already assigned or invalid</response>
    /// <response code="404">Role or permission not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{id:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddPermissionToRole(Guid id, Guid permissionId)
    {
        _logger.LogInformation("Adding permission {PermissionId} to system role {RoleId}", permissionId, id);

        var command = new AddPermissionToRoleCommand(id, permissionId);
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

        _logger.LogInformation("Permission {PermissionId} added to system role {RoleId} successfully", permissionId, id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Removes a permission from a system role.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Permission removed successfully</response>
    /// <response code="400">Permission not assigned to role</response>
    /// <response code="404">Role or permission not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemovePermissionFromRole(Guid id, Guid permissionId)
    {
        _logger.LogInformation("Removing permission {PermissionId} from system role {RoleId}", permissionId, id);

        var command = new RemovePermissionFromRoleCommand(id, permissionId);
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

        _logger.LogInformation("Permission {PermissionId} removed from system role {RoleId} successfully", permissionId, id);
        return Ok(new { success = true, message = result.Value });
    }
}

/// <summary>
/// Request model for creating a system role.
/// </summary>
public record CreateRoleRequest(
    string Name,
    string Description);

/// <summary>
/// Request model for updating a system role.
/// </summary>
public record UpdateRoleRequest(
    string Name,
    string Description);
