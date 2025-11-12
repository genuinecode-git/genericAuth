using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Handler for updating an existing system role.
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<UpdateRoleCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateRoleCommandResponse>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Find the role
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<UpdateRoleCommandResponse>.Failure(
                $"Role with ID '{request.RoleId}' not found.");
        }

        // Check if the new name conflicts with another role
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r =>
                r.Name.ToLower() == request.Name.ToLower() &&
                r.Id != request.RoleId,
                cancellationToken);

        if (existingRole != null)
        {
            return Result<UpdateRoleCommandResponse>.Failure(
                $"A role with the name '{request.Name}' already exists.");
        }

        try
        {
            // Update the role
            role.Update(request.Name, request.Description);

            await _context.SaveChangesAsync(cancellationToken);

            var response = new UpdateRoleCommandResponse(
                RoleId: role.Id,
                Name: role.Name,
                Description: role.Description,
                IsActive: role.IsActive,
                Message: "System role updated successfully.");

            return Result<UpdateRoleCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UpdateRoleCommandResponse>.Failure(ex.Message);
        }
    }
}
