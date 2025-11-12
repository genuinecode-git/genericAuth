using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Handler for deleting a system role.
/// </summary>
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Find the role with user assignments
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<string>.Failure(
                $"Role with ID '{request.RoleId}' not found.");
        }

        // Check if role has user assignments
        if (role.UserRoles.Any())
        {
            return Result<string>.Failure(
                $"Cannot delete role '{role.Name}' because it has {role.UserRoles.Count} user assignment(s). " +
                "Please remove all user assignments before deleting the role.");
        }

        try
        {
            // Soft delete by deactivating (recommended) or hard delete
            // For now, we'll use soft delete by deactivating
            if (role.IsActive)
            {
                role.Deactivate();
            }

            // Alternatively, for hard delete:
            // _context.Roles.Remove(role);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Role '{role.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
