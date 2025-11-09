using Asp.Versioning;
using GenericAuth.Application.Features.Authentication.Commands.ForgotPassword;
using GenericAuth.Application.Features.Authentication.Commands.Login;
using GenericAuth.Application.Features.Authentication.Commands.Logout;
using GenericAuth.Application.Features.Authentication.Commands.Register;
using GenericAuth.Application.Features.Authentication.Commands.RefreshToken;
using GenericAuth.Application.Features.Authentication.Commands.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GenericAuth.API.Controllers.V1;

/// <summary>
/// Controller for authentication operations including login, registration, password management, and token refresh.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT access token and refresh token</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid credentials or inactive account</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var command = new LoginCommand(request.Email, request.Password, request.ApplicationId);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Login successful for email: {Email}", request.Email);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Created user information</returns>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Invalid request or email already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Registration successful for email: {Email}", request.Email);
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New JWT access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Token refresh failed. Errors: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Token refreshed successfully");
        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Logs out a user by revoking refresh tokens.
    /// If no refresh token is provided, all tokens for the user will be revoked.
    /// </summary>
    /// <param name="request">Optional refresh token to revoke</param>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { errors = new[] { "Invalid user token." } });
        }

        _logger.LogInformation("Logout attempt for user: {UserId}", userId);

        var command = new LogoutCommand(userId, request?.RefreshToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Logout failed for user: {UserId}. Errors: {Errors}",
                userId, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Logout successful for user: {UserId}", userId);
        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Initiates the password reset process by generating a reset token.
    /// </summary>
    /// <param name="request">Email address</param>
    /// <returns>Confirmation message (does not reveal if email exists)</returns>
    /// <response code="200">Reset email will be sent if email exists</response>
    /// <response code="400">Invalid email format</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

        var command = new ForgotPasswordCommand(request.Email);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Password reset request failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { success = true, message = result.Value });
    }

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="request">Email, reset token, and new password</param>
    /// <returns>Password reset confirmation</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid token or password validation failed</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

        var command = new ResetPasswordCommand(request.Email, request.ResetToken, request.NewPassword);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Password reset failed for email: {Email}. Errors: {Errors}",
                request.Email, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }

        _logger.LogInformation("Password reset successful for email: {Email}", request.Email);
        return Ok(new { success = true, message = result.Value });
    }
}

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest(
    string Email,
    string Password,
    Guid? ApplicationId = null);

/// <summary>
/// Request model for user registration.
/// </summary>
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

/// <summary>
/// Request model for token refresh.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Request model for logout.
/// </summary>
public record LogoutRequest(string? RefreshToken = null);

/// <summary>
/// Request model for forgot password.
/// </summary>
public record ForgotPasswordRequest(string Email);

/// <summary>
/// Request model for password reset.
/// </summary>
public record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword);
