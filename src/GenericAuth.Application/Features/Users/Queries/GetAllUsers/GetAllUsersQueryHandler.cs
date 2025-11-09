using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<UserDto>>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.Value.ToLower().Contains(searchTerm));
        }

        // Apply user type filter
        if (!string.IsNullOrWhiteSpace(request.UserType))
        {
            if (Enum.TryParse<UserType>(request.UserType, true, out var userType))
            {
                query = query.Where(u => u.UserType == userType);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project to DTO
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email.Value,
                u.UserType.ToString(),
                u.IsActive,
                u.IsEmailConfirmed,
                u.LastLoginAt,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<UserDto>(
            users,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<UserDto>>.Success(paginatedList);
    }
}
