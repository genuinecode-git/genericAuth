using Asp.Versioning;
using GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;
using GenericAuth.Application.Features.Applications.Commands.ChangeUserApplicationRole;
using GenericAuth.Application.Features.Applications.Commands.RemoveUserFromApplication;
using GenericAuth.Application.Features.Applications.Queries.GetApplicationUsers;
using GenericAuth.Application.Features.Applications.Queries.GetUserApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers.V1;

/// <summary>
/// Controller for managing user assignments to applications.
/// Only accessible by Auth Admin users.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-applications")]
[Authorize(Policy = "AuthAdminOnly")]
[Produces("application/json")]
public class UserApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserApplicationsController> _logger;

    public UserApplicationsController(IMediator mediator, ILogger<UserApplicationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Assigns a user to an application with a specific role.
    /// If RoleName is not provided, the default role for the application will be used.
    /// </summary>
    /// <param name="command">User assignment details</param>
    /// <returns>Assignment confirmation</returns>
    /// <response code="200">User assigned successfully</response>
    /// <response code="400">Invalid request or assignment failed</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    /// <response code="404">User, application, or role not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(AssignUserToApplicationCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignUserToApplication([FromBody] AssignUserToApplicationCommand command)
    {
        _logger.LogInformation(
            "Assigning user {UserId} to application {ApplicationCode} with role {RoleName}",
            command.UserId, command.ApplicationCode, command.RoleName ?? "(default)");

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to assign user {UserId} to application {ApplicationCode}: {Errors}",
                command.UserId, command.ApplicationCode, string.Join(", ", result.Errors));

            // Determine appropriate status code based on error message
            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation(
            "User {UserId} assigned successfully to application {ApplicationCode}",
            command.UserId, command.ApplicationCode);

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Gets all applications assigned to a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's application assignments</returns>
    /// <response code="200">Applications retrieved successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("users/{userId:guid}/applications")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserApplications(Guid userId)
    {
        _logger.LogInformation("Getting applications for user {UserId}", userId);

        var query = new GetUserApplicationsQuery(userId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to get applications for user {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Gets all users assigned to a specific application with pagination.
    /// </summary>
    /// <param name="appCode">Application code</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="searchTerm">Search term for filtering by email, name, or role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of application users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="404">Application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("applications/{appCode}/users")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApplicationUsers(
        string appCode,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        _logger.LogInformation(
            "Getting users for application {ApplicationCode} - Page: {PageNumber}, Size: {PageSize}",
            appCode, pageNumber, pageSize);

        var query = new GetApplicationUsersQuery(appCode, pageNumber, pageSize, searchTerm, isActive);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to get users for application {ApplicationCode}: {Errors}",
                appCode, string.Join(", ", result.Errors));
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
    /// Changes a user's role within an application.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appCode">Application code</param>
    /// <param name="request">New role information</param>
    /// <returns>Role change confirmation</returns>
    /// <response code="200">Role changed successfully</response>
    /// <response code="400">Invalid request or role change failed</response>
    /// <response code="404">User or application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPut("users/{userId:guid}/applications/{appCode}/role")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeUserApplicationRole(
        Guid userId,
        string appCode,
        [FromBody] ChangeUserRoleRequest request)
    {
        _logger.LogInformation(
            "Changing role for user {UserId} in application {ApplicationCode} to role {RoleId}",
            userId, appCode, request.NewApplicationRoleId);

        var command = new ChangeUserApplicationRoleCommand(userId, appCode, request.NewApplicationRoleId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to change user role: {Errors}", string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("User role changed successfully");
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Removes a user from an application.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appCode">Application code</param>
    /// <returns>Removal confirmation</returns>
    /// <response code="200">User removed successfully</response>
    /// <response code="400">Removal failed</response>
    /// <response code="404">User or application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpDelete("users/{userId:guid}/applications/{appCode}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveUserFromApplication(Guid userId, string appCode)
    {
        _logger.LogInformation("Removing user {UserId} from application {ApplicationCode}", userId, appCode);

        var command = new RemoveUserFromApplicationCommand(userId, appCode);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove user from application: {Errors}",
                string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = result.Errors });
            }

            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("User {UserId} removed successfully from application {ApplicationCode}",
            userId, appCode);
        return Ok(new { success = true, message = result.Value });
    }
}

/// <summary>
/// Request model for changing user's application role.
/// </summary>
public record ChangeUserRoleRequest(Guid NewApplicationRoleId);
