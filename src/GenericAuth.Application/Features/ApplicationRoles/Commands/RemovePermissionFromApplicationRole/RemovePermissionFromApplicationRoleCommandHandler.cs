using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.RemovePermissionFromApplicationRole;

/// <summary>
/// Handler for removing a permission from an application role.
/// </summary>
public class RemovePermissionFromApplicationRoleCommandHandler : IRequestHandler<RemovePermissionFromApplicationRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RemovePermissionFromApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        RemovePermissionFromApplicationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Load the application with its roles
        var application = await _context.Applications
            .Include(a => a.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            return Result<string>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        // Verify permission exists
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            return Result<string>.Failure(
                $"Permission with ID '{request.PermissionId}' not found.");
        }

        try
        {
            // Get the role from the application
            var role = application.GetRole(request.RoleId);

            // Remove the permission from the role
            role.RemovePermission(request.PermissionId, null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Permission '{permission.Name}' removed from role '{role.Name}'.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
