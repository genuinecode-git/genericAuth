using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.RemoveRoleFromUser;

/// <summary>
/// Handler for removing a system role from an Auth Admin user.
/// </summary>
public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RemoveRoleFromUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        RemoveRoleFromUserCommand request,
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
                "System roles can only be removed from Auth Admin users.");
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
            user.RemoveRole(request.RoleId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"Role '{role.Name}' removed from user '{user.Email.Value}' successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
