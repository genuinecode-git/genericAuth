using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.SetDefaultApplicationRole;

/// <summary>
/// Handler for setting an application role as default.
/// </summary>
public class SetDefaultApplicationRoleCommandHandler : IRequestHandler<SetDefaultApplicationRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public SetDefaultApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        SetDefaultApplicationRoleCommand request,
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

            // Auto-activate the role if it's inactive
            // Setting a role as default implies it should be active
            if (!role.IsActive)
            {
                role.Activate(null); // TODO: Get from current user context
            }

            // Remove default status from current default role
            var currentDefault = application.Roles.FirstOrDefault(r => r.IsDefault);
            if (currentDefault != null && currentDefault.Id != request.RoleId)
            {
                currentDefault.RemoveDefaultStatus(null); // TODO: Get from current user context
            }

            // Set the new default
            role.SetAsDefault(null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Role '{role.Name}' is now the default role for this application.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
