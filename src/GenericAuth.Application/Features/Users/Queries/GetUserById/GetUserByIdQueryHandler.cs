using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDetailDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserApplications)
            .ThenInclude(ua => ua.ApplicationRole)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UserDetailDto>.Failure("User not found");
        }

        // Get application details for user assignments
        var applicationIds = user.UserApplications.Select(ua => ua.ApplicationId).ToList();
        var applications = await _context.Applications
            .Where(a => applicationIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var userApplications = user.UserApplications.Select(ua => new UserApplicationDto(
            ua.ApplicationId,
            applications.ContainsKey(ua.ApplicationId) ? applications[ua.ApplicationId].Code.Value : "N/A",
            applications.ContainsKey(ua.ApplicationId) ? applications[ua.ApplicationId].Name : "N/A",
            ua.ApplicationRole.Name,
            ua.IsActive,
            ua.AssignedAt,
            ua.LastAccessedAt
        )).ToList();

        var userDetail = new UserDetailDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            user.UserType.ToString(),
            user.IsActive,
            user.IsEmailConfirmed,
            user.LastLoginAt,
            user.CreatedAt,
            user.UpdatedAt,
            userApplications);

        return Result<UserDetailDto>.Success(userDetail);
    }
}
