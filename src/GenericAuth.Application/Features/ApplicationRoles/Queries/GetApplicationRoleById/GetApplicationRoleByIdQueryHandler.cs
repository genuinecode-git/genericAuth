using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoleById;

/// <summary>
/// Handler for getting detailed information about a specific application role.
/// </summary>
public class GetApplicationRoleByIdQueryHandler : IRequestHandler<GetApplicationRoleByIdQuery, Result<ApplicationRoleDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationRoleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ApplicationRoleDetailDto>> Handle(
        GetApplicationRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Verify application exists
        var applicationExists = await _context.Applications
            .AnyAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (!applicationExists)
        {
            return Result<ApplicationRoleDetailDto>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        // Get the role with its permissions
        var role = await _context.ApplicationRoles
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.ApplicationId == request.ApplicationId, cancellationToken);

        if (role == null)
        {
            return Result<ApplicationRoleDetailDto>.Failure(
                $"Role with ID '{request.RoleId}' not found in application '{request.ApplicationId}'.");
        }

        // Get user count for this role
        var userCount = await _context.UserApplications
            .CountAsync(ua => ua.ApplicationRoleId == request.RoleId, cancellationToken);

        // Map to DTO
        var permissions = role.Permissions
            .Select(p => new PermissionDto(
                p.Permission.Id,
                p.Permission.Resource,
                p.Permission.Action,
                p.Permission.Name,
                p.Permission.Description))
            .ToList();

        var roleDetailDto = new ApplicationRoleDetailDto(
            Id: role.Id,
            ApplicationId: role.ApplicationId,
            Name: role.Name,
            Description: role.Description,
            IsActive: role.IsActive,
            IsDefault: role.IsDefault,
            CreatedAt: role.CreatedAt,
            UpdatedAt: role.UpdatedAt,
            Permissions: permissions,
            UserCount: userCount);

        return Result<ApplicationRoleDetailDto>.Success(roleDetailDto);
    }
}
