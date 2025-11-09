using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeleteApplicationRole;

/// <summary>
/// Handler for deleting an application role.
/// </summary>
public class DeleteApplicationRoleCommandHandler : IRequestHandler<DeleteApplicationRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeleteApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        DeleteApplicationRoleCommand request,
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

            // Prevent deletion of default role
            if (role.IsDefault)
            {
                return Result<string>.Failure(
                    "Cannot delete the default role. Please set another role as default first.");
            }

            // Check if any users are assigned to this role
            var userCount = await _context.UserApplications
                .CountAsync(ua => ua.ApplicationRoleId == request.RoleId, cancellationToken);

            if (userCount > 0)
            {
                return Result<string>.Failure(
                    $"Cannot delete role. {userCount} user(s) are currently assigned to this role. " +
                    "Please reassign these users to a different role first.");
            }

            // Remove the role
            _context.ApplicationRoles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Application role deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
