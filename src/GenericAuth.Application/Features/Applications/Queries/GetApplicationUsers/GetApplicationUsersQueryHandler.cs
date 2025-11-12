using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Applications.Queries.GetApplicationUsers;

/// <summary>
/// Handler for getting all users assigned to an application.
/// </summary>
public class GetApplicationUsersQueryHandler : IRequestHandler<GetApplicationUsersQuery, Result<PaginatedList<ApplicationUserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ApplicationUserDto>>> Handle(
        GetApplicationUsersQuery request,
        CancellationToken cancellationToken)
    {
        // Verify application exists
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Code.Value == request.ApplicationCode.ToUpperInvariant(), cancellationToken);

        if (application == null)
        {
            return Result<PaginatedList<ApplicationUserDto>>.Failure(
                $"Application with code '{request.ApplicationCode}' not found.");
        }

        // Build query
        var query = _context.UserApplications
            .Include(ua => ua.User)
            .Include(ua => ua.ApplicationRole)
            .Where(ua => ua.ApplicationId == application.Id)
            .AsQueryable();

        // Apply search filter - using case-insensitive contains
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(ua =>
                EF.Functions.Like(ua.User.Email, $"%{request.SearchTerm}%") ||
                EF.Functions.Like(ua.User.FirstName, $"%{request.SearchTerm}%") ||
                EF.Functions.Like(ua.User.LastName, $"%{request.SearchTerm}%") ||
                EF.Functions.Like(ua.ApplicationRole.Name, $"%{request.SearchTerm}%"));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(ua => ua.IsActive == request.IsActive.Value);
        }

        // Order by assigned date (newest first)
        query = query.OrderByDescending(ua => ua.AssignedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to DTO
        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ua => new ApplicationUserDto(
                ua.UserId,
                ua.User.Email,
                ua.User.FirstName,
                ua.User.LastName,
                ua.User.UserType.ToString(),
                ua.ApplicationRoleId,
                ua.ApplicationRole.Name,
                ua.ApplicationRole.IsActive,
                ua.ApplicationRole.IsDefault,
                ua.AssignedAt,
                ua.IsActive))
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<ApplicationUserDto>(
            users,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<ApplicationUserDto>>.Success(paginatedList);
    }
}
