using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoles;

/// <summary>
/// Handler for getting application roles with pagination and filtering.
/// </summary>
public class GetApplicationRolesQueryHandler : IRequestHandler<GetApplicationRolesQuery, Result<PaginatedList<ApplicationRoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ApplicationRoleDto>>> Handle(
        GetApplicationRolesQuery request,
        CancellationToken cancellationToken)
    {
        // Verify application exists
        var applicationExists = await _context.Applications
            .AnyAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (!applicationExists)
        {
            return Result<PaginatedList<ApplicationRoleDto>>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        // Build query
        var query = _context.ApplicationRoles
            .Where(r => r.ApplicationId == request.ApplicationId)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(searchTerm) ||
                r.Description.ToLower().Contains(searchTerm));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        // Order by: Default first, then by name
        query = query.OrderByDescending(r => r.IsDefault)
                     .ThenBy(r => r.Name);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var roles = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ApplicationRoleDto(
                r.Id,
                r.ApplicationId,
                r.Name,
                r.Description,
                r.IsActive,
                r.IsDefault,
                r.CreatedAt,
                r.UpdatedAt))
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<ApplicationRoleDto>(
            roles,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<ApplicationRoleDto>>.Success(paginatedList);
    }
}
