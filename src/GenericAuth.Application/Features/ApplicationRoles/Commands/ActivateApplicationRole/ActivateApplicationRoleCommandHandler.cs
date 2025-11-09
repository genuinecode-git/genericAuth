using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.ActivateApplicationRole;

/// <summary>
/// Handler for activating an application role.
/// </summary>
public class ActivateApplicationRoleCommandHandler : IRequestHandler<ActivateApplicationRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ActivateApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        ActivateApplicationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Load the application with its roles
        var application = await _context.Applications
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            return Result<string>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        try
        {
            // Get the role from the application
            var role = application.GetRole(request.RoleId);

            // Activate the role
            role.Activate(null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Role '{role.Name}' has been activated.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
