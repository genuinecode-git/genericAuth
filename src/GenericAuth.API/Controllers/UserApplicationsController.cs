using GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers;

/// <summary>
/// Controller for managing user assignments to applications.
/// Only accessible by Auth Admin users.
/// </summary>
[ApiController]
[Route("api/user-applications")]
[Authorize(Policy = "AuthAdminOnly")] // TODO: Implement this policy
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
            command.UserId, command.ApplicationCode, command.RoleName);

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

    // TODO: Add more endpoints:
    // - GET /api/user-applications/users/{userId}/applications - Get user's applications
    // - GET /api/user-applications/applications/{appCode}/users - Get application's users
    // - PUT /api/user-applications/users/{userId}/applications/{appCode}/role - Change user's role
    // - DELETE /api/user-applications/users/{userId}/applications/{appCode} - Remove user from application
    // - POST /api/user-applications/users/{userId}/applications/{appCode}/activate - Activate access
    // - POST /api/user-applications/users/{userId}/applications/{appCode}/deactivate - Deactivate access
}
