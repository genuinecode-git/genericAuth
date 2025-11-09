using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Handler for retrieving paginated system roles.
/// </summary>
public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<PaginatedList<RoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<RoleDto>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(searchTerm) ||
                r.Description.ToLower().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        // Order by name
        query = query.OrderBy(r => r.Name);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to DTO
        var roles = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsActive,
                r.RolePermissions.Count,
                r.UserRoles.Count,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<RoleDto>(
            roles,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<RoleDto>>.Success(paginatedList);
    }
}
