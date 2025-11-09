using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeactivateApplicationRole;

/// <summary>
/// Handler for deactivating an application role.
/// </summary>
public class DeactivateApplicationRoleCommandHandler : IRequestHandler<DeactivateApplicationRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        DeactivateApplicationRoleCommand request,
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

            // Prevent deactivation of default role
            if (role.IsDefault)
            {
                return Result<string>.Failure(
                    "Cannot deactivate the default role. Please set another role as default first.");
            }

            // Deactivate the role
            role.Deactivate(null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Role '{role.Name}' has been deactivated.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
