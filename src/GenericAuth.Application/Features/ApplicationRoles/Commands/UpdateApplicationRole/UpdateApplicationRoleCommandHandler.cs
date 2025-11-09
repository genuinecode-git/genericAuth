using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.UpdateApplicationRole;

/// <summary>
/// Handler for updating an application role.
/// </summary>
public class UpdateApplicationRoleCommandHandler : IRequestHandler<UpdateApplicationRoleCommand, Result<UpdateApplicationRoleCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateApplicationRoleCommandResponse>> Handle(
        UpdateApplicationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Load the application with its roles
        var application = await _context.Applications
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            return Result<UpdateApplicationRoleCommandResponse>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        try
        {
            // Get the role from the application
            var role = application.GetRole(request.RoleId);

            // Update the role
            role.Update(
                name: request.Name,
                description: request.Description,
                updatedBy: null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            var response = new UpdateApplicationRoleCommandResponse(
                RoleId: role.Id,
                Name: role.Name,
                Description: role.Description,
                Message: "Application role updated successfully.");

            return Result<UpdateApplicationRoleCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UpdateApplicationRoleCommandResponse>.Failure(ex.Message);
        }
    }
}
