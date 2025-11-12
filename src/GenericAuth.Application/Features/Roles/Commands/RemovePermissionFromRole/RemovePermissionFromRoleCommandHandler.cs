using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.RemovePermissionFromRole;

/// <summary>
/// Handler for removing a permission from a system role.
/// </summary>
public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RemovePermissionFromRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        RemovePermissionFromRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Load the role with its permissions
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<string>.Failure(
                $"Role with ID '{request.RoleId}' not found.");
        }

        // Check if permission exists
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            return Result<string>.Failure(
                $"Permission with ID '{request.PermissionId}' not found.");
        }

        try
        {
            role.RemovePermission(request.PermissionId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"Permission '{permission.Name}' removed from role '{role.Name}' successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
