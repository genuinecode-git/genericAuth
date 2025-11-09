using Asp.Versioning;
using GenericAuth.Application.Features.Users.Commands.ActivateUser;
using GenericAuth.Application.Features.Users.Commands.AssignRoleToUser;
using GenericAuth.Application.Features.Users.Commands.DeactivateUser;
using GenericAuth.Application.Features.Users.Commands.RemoveRoleFromUser;
using GenericAuth.Application.Features.Users.Commands.UpdateUser;
using GenericAuth.Application.Features.Users.Queries.GetAllUsers;
using GenericAuth.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers.V1;

/// <summary>
/// Controller for managing users.
/// Only accessible by Auth Admin users.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AuthAdminOnly")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> _logger)
    {
        _mediator = mediator;
        this._logger = _logger;
    }

    /// <summary>
    /// Gets a paginated list of all users.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Search term for filtering by name or email</param>
    /// <param name="userType">Filter by user type (Regular or AuthAdmin)</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? userType = null)
    {
        _logger.LogInformation(
            "Getting users - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}, Type: {UserType}",
            pageNumber, pageSize, searchTerm, userType);

        var query = new GetAllUsersQuery(pageNumber, pageSize, searchTerm, userType);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
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
    /// Gets a user by ID with detailed information.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details including application assignments</returns>
    /// <response code="200">User found</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", id);

        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("User not found: {UserId}", id);
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="command">Update user details</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var command = new UpdateUserCommand(id, request.FirstName, request.LastName);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update user {UserId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("User updated successfully: {UserId}", id);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Activation confirmation</returns>
    /// <response code="200">User activated successfully</response>
    /// <response code="400">User is already active or activation failed</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        _logger.LogInformation("Activating user: {UserId}", id);

        var command = new ActivateUserCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate user {UserId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("User activated successfully: {UserId}", id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Deactivation confirmation</returns>
    /// <response code="200">User deactivated successfully</response>
    /// <response code="400">User is already inactive or deactivation failed</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        _logger.LogInformation("Deactivating user: {UserId}", id);

        var command = new DeactivateUserCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate user {UserId}: {Errors}", id, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("User deactivated successfully: {UserId}", id);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Assigns a system role to an Auth Admin user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Assignment confirmation</returns>
    /// <response code="200">Role assigned successfully</response>
    /// <response code="400">User is not Auth Admin or role already assigned</response>
    /// <response code="404">User or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost("{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId)
    {
        _logger.LogInformation("Assigning role {RoleId} to user {UserId}", roleId, userId);

        var command = new AssignRoleToUserCommand(userId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to assign role: {Errors}", string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} assigned to user {UserId} successfully", roleId, userId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Removes a system role from an Auth Admin user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <returns>Removal confirmation</returns>
    /// <response code="200">Role removed successfully</response>
    /// <response code="400">User does not have this role or user is not Auth Admin</response>
    /// <response code="404">User or role not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId}", roleId, userId);

        var command = new RemoveRoleFromUserCommand(userId, roleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove role: {Errors}", string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Role {RoleId} removed from user {UserId} successfully", roleId, userId);
        return Ok(new { success = true, message = result.Value });
    }
}

/// <summary>
/// Request model for updating user profile.
/// </summary>
public record UpdateUserRequest(string FirstName, string LastName);
