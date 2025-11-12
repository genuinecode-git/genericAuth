using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.AssignRoleToUser;

/// <summary>
/// Handler for assigning a system role to an Auth Admin user.
/// </summary>
public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleToUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        AssignRoleToUserCommand request,
        CancellationToken cancellationToken)
    {
        // Load user with roles
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure(
                $"User with ID '{request.UserId}' not found.");
        }

        // Verify user is AuthAdmin
        if (!user.IsAuthAdmin())
        {
            return Result<string>.Failure(
                "System roles can only be assigned to Auth Admin users. " +
                "Regular users use application-scoped roles instead.");
        }

        // Check if role exists
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<string>.Failure(
                $"Role with ID '{request.RoleId}' not found.");
        }

        try
        {
            // Create UserRole association
            var userRole = new UserRole(request.UserId, request.RoleId);
            user.AddRole(userRole);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"Role '{role.Name}' assigned to user '{user.Email.Value}' successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
