using GenericAuth.Application.Features.Applications.Commands.CreateApplication;
using GenericAuth.Application.Features.Applications.Queries.GetApplicationByCode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericAuth.API.Controllers;

/// <summary>
/// Controller for managing applications.
/// Only accessible by Auth Admin users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AuthAdminOnly")] // TODO: Implement this policy
[Produces("application/json")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IMediator mediator, ILogger<ApplicationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new application with initial roles.
    /// Returns the plain text API key - this is the ONLY time it will be shown!
    /// </summary>
    /// <param name="command">Application creation details including initial roles</param>
    /// <returns>Application details including the plain text API key</returns>
    /// <response code="200">Application created successfully</response>
    /// <response code="400">Invalid request or application code already exists</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApplicationCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationCommand command)
    {
        _logger.LogInformation("Creating application: {ApplicationCode}", command.Code);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create application {ApplicationCode}: {Errors}",
                command.Code, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Application created successfully: {ApplicationCode}", command.Code);

        return Ok(new
        {
            success = true,
            data = result.Value,
            warning = "⚠️ IMPORTANT: Store the API key securely - it will not be shown again!"
        });
    }

    /// <summary>
    /// Gets an application by its unique code.
    /// </summary>
    /// <param name="code">The application code</param>
    /// <returns>Application details</returns>
    /// <response code="200">Application found</response>
    /// <response code="404">Application not found</response>
    /// <response code="401">Unauthorized - Auth Admin access required</response>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApplicationByCode(string code)
    {
        _logger.LogInformation("Getting application by code: {ApplicationCode}", code);

        var result = await _mediator.Send(new GetApplicationByCodeQuery(code));

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Application not found: {ApplicationCode}", code);
            return NotFound(new { errors = result.Errors });
        }

        return Ok(new { success = true, data = result.Value });
    }

    // TODO: Add more endpoints:
    // - GET /api/applications - List all applications
    // - GET /api/applications/{id} - Get application by ID
    // - PUT /api/applications/{id} - Update application
    // - POST /api/applications/{id}/regenerate-api-key - Regenerate API key
    // - POST /api/applications/{id}/activate - Activate application
    // - POST /api/applications/{id}/deactivate - Deactivate application
    // - POST /api/applications/{id}/roles - Create role for application
    // - GET /api/applications/{id}/roles - Get roles for application
}
