using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Applications.Queries.GetUserApplications;

/// <summary>
/// Handler for getting all applications assigned to a user.
/// </summary>
public class GetUserApplicationsQueryHandler : IRequestHandler<GetUserApplicationsQuery, Result<List<UserApplicationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserApplicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserApplicationDto>>> Handle(
        GetUserApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Result<List<UserApplicationDto>>.Failure(
                $"User with ID '{request.UserId}' not found.");
        }

        // Get user's application assignments
        var userApplications = await _context.UserApplications
            .Include(ua => ua.Application)
            .Include(ua => ua.ApplicationRole)
            .Where(ua => ua.UserId == request.UserId)
            .OrderBy(ua => ua.Application.Name)
            .Select(ua => new UserApplicationDto(
                ua.ApplicationId,
                ua.Application.Code.Value,
                ua.Application.Name,
                ua.Application.IsActive,
                ua.ApplicationRoleId,
                ua.ApplicationRole.Name,
                ua.ApplicationRole.IsActive,
                ua.ApplicationRole.IsDefault,
                ua.AssignedAt,
                ua.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<UserApplicationDto>>.Success(userApplications);
    }
}
