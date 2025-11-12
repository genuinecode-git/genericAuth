using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Applications.Commands.ChangeUserApplicationRole;

/// <summary>
/// Handler for changing a user's role within an application.
/// </summary>
public class ChangeUserApplicationRoleCommandHandler : IRequestHandler<ChangeUserApplicationRoleCommand, Result<ChangeUserApplicationRoleCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public ChangeUserApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ChangeUserApplicationRoleCommandResponse>> Handle(
        ChangeUserApplicationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.NotFound(
                $"User with ID '{request.UserId}' not found.");
        }

        // Get the application with its roles and user assignments
        var application = await _context.Applications
            .Include(a => a.Roles)
            .Include(a => a.UserApplications)
            .FirstOrDefaultAsync(a => a.Code.Value == request.ApplicationCode.ToUpperInvariant(), cancellationToken);

        if (application == null)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.NotFound(
                $"Application with code '{request.ApplicationCode}' not found.");
        }

        if (!application.IsActive)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.Failure(
                $"Application '{request.ApplicationCode}' is not active.");
        }

        // Get the new role
        var newRole = application.Roles.FirstOrDefault(r => r.Id == request.NewApplicationRoleId);

        if (newRole == null)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.Failure(
                $"Role with ID '{request.NewApplicationRoleId}' not found in application '{request.ApplicationCode}'.");
        }

        if (!newRole.IsActive)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.Failure(
                $"Role '{newRole.Name}' is not active.");
        }

        // Change the user's role
        try
        {
            application.ChangeUserRole(
                userId: user.Id,
                newRoleId: newRole.Id,
                updatedBy: null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ChangeUserApplicationRoleCommandResponse(
                UserId: user.Id,
                ApplicationId: application.Id,
                NewRoleId: newRole.Id,
                NewRoleName: newRole.Name,
                Message: $"User's role in application '{application.Name}' changed to '{newRole.Name}'.");

            return Result<ChangeUserApplicationRoleCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<ChangeUserApplicationRoleCommandResponse>.Failure(ex.Message);
        }
    }
}
