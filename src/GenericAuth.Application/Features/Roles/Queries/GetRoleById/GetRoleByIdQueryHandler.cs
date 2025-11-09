using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Handler for retrieving detailed information about a specific system role.
/// </summary>
public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RoleDetailDto>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<RoleDetailDto>.Failure($"Role with ID '{request.RoleId}' not found.");
        }

        // Map permissions
        var permissions = role.RolePermissions
            .Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.Description))
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .ToList();

        // Map users
        var users = role.UserRoles
            .Select(ur => new RoleUserDto(
                ur.User.Id,
                ur.User.FirstName,
                ur.User.LastName,
                ur.User.Email.Value,
                ur.AssignedAt))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToList();

        var roleDetail = new RoleDetailDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            permissions,
            users);

        return Result<RoleDetailDto>.Success(roleDetail);
    }
}
