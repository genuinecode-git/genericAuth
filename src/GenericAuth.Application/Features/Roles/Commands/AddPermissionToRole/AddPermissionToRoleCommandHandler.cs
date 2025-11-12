using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.AddPermissionToRole;

/// <summary>
/// Handler for adding a permission to a system role.
/// </summary>
public class AddPermissionToRoleCommandHandler : IRequestHandler<AddPermissionToRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public AddPermissionToRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        AddPermissionToRoleCommand request,
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
            // Create RolePermission association
            var rolePermission = new RolePermission(request.RoleId, request.PermissionId);
            role.AddPermission(rolePermission);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"Permission '{permission.Name}' added to role '{role.Name}' successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
