using GenericAuth.Application.Features.Applications.Queries.GetApplicationByCode;
using MediatR;
using Microsoft.EntityFrameworkCore;
using GenericAuth.Application.Common.Interfaces;

namespace GenericAuth.API.Middleware;

/// <summary>
/// Middleware to authenticate external applications using application_code and api_key.
/// Validates the application before user authentication can proceed.
/// </summary>
public class ApplicationAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApplicationAuthenticationMiddleware> _logger;

    public ApplicationAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApplicationAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IMediator mediator,
        IApplicationDbContext dbContext)
    {
        // Skip middleware for certain paths (health checks, swagger, etc.)
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/health") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/api/openapi") ||
            path.StartsWith("/weatherforecast"))
        {
            await _next(context);
            return;
        }

        // Check if application headers are present
        var applicationCode = context.Request.Headers["X-Application-Code"].FirstOrDefault();
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

        // If headers are not present, this might be an auth admin request
        // or an endpoint that doesn't require application context
        if (string.IsNullOrEmpty(applicationCode) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogInformation("No application authentication headers present for path: {Path}", path);
            await _next(context);
            return;
        }

        try
        {
            // Query the application
            var result = await mediator.Send(new GetApplicationByCodeQuery(applicationCode));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Application not found: {ApplicationCode}", applicationCode);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid application credentials",
                    message = "Application not found or invalid"
                });
                return;
            }

            var applicationDto = result.Value;

            if (!applicationDto.IsActive)
            {
                _logger.LogWarning("Inactive application attempted access: {ApplicationCode}", applicationCode);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Application inactive",
                    message = "This application is currently inactive"
                });
                return;
            }

            // Validate the API key
            // We need to fetch the full application entity to validate the hashed key
            var application = await dbContext.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationDto.Id);

            if (application == null || !application.ValidateApiKey(apiKey))
            {
                _logger.LogWarning("Invalid API key for application: {ApplicationCode}", applicationCode);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid application credentials",
                    message = "Invalid API key"
                });
                return;
            }

            // Application authenticated successfully - add to HttpContext
            context.Items["Application"] = application;
            context.Items["ApplicationId"] = application.Id;
            context.Items["ApplicationCode"] = application.Code.Value;

            _logger.LogInformation("Application authenticated successfully: {ApplicationCode}", applicationCode);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application authentication for: {ApplicationCode}", applicationCode);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Authentication error",
                message = "An error occurred during application authentication"
            });
        }
    }
}

/// <summary>
/// Extension methods for ApplicationAuthenticationMiddleware.
/// </summary>
public static class ApplicationAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApplicationAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApplicationAuthenticationMiddleware>();
    }
}
