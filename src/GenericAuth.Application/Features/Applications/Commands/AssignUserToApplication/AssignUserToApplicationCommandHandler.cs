using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;

public class AssignUserToApplicationCommandHandler : IRequestHandler<AssignUserToApplicationCommand, Result<AssignUserToApplicationCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public AssignUserToApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AssignUserToApplicationCommandResponse>> Handle(
        AssignUserToApplicationCommand request,
        CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _context.Users
            .Include(u => u.UserApplications)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<AssignUserToApplicationCommandResponse>.NotFound(
                $"User with ID '{request.UserId}' not found.");
        }

        // Get the application with its roles
        var application = await _context.Applications
            .Include(a => a.Roles)
            .Include(a => a.UserApplications)
            .FirstOrDefaultAsync(a => a.Code.Value == request.ApplicationCode.ToUpperInvariant(), cancellationToken);

        if (application == null)
        {
            return Result<AssignUserToApplicationCommandResponse>.NotFound(
                $"Application with code '{request.ApplicationCode}' not found.");
        }

        if (!application.IsActive)
        {
            return Result<AssignUserToApplicationCommandResponse>.Failure(
                $"Application '{request.ApplicationCode}' is not active.");
        }

        // Get the role by name within this application, or use default role if not specified
        Domain.Entities.ApplicationRole? role;

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            // Use the default role
            role = application.Roles.FirstOrDefault(r => r.IsDefault);

            if (role == null)
            {
                return Result<AssignUserToApplicationCommandResponse>.Failure(
                    $"No default role configured for application '{request.ApplicationCode}'. Please specify a role name.");
            }
        }
        else
        {
            // Get the role by name
            role = application.Roles
                .FirstOrDefault(r => r.Name.Equals(request.RoleName, StringComparison.OrdinalIgnoreCase));

            if (role == null)
            {
                return Result<AssignUserToApplicationCommandResponse>.Failure(
                    $"Role '{request.RoleName}' not found in application '{request.ApplicationCode}'.");
            }
        }

        if (!role.IsActive)
        {
            return Result<AssignUserToApplicationCommandResponse>.Failure(
                $"Role '{role.Name}' is not active.");
        }

        // Assign user to application with the specified role
        try
        {
            application.AssignUser(
                userId: user.Id,
                applicationRoleId: role.Id,
                assignedBy: null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            var response = new AssignUserToApplicationCommandResponse(
                UserId: user.Id,
                ApplicationId: application.Id,
                RoleId: role.Id,
                Message: $"User successfully assigned to application '{application.Name}' with role '{role.Name}'.");

            return Result<AssignUserToApplicationCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<AssignUserToApplicationCommandResponse>.Failure(ex.Message);
        }
    }
}
