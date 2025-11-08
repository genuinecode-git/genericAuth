using Dapper;
using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<List<UserListDto>>>
{
    private readonly IQueryDbConnection _queryDb;

    public GetUsersQueryHandler(IQueryDbConnection queryDb)
    {
        _queryDb = queryDb;
    }

    public async Task<Result<List<UserListDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Use Dapper for optimized read-only query
        const string sql = @"
            SELECT
                Id,
                FirstName,
                LastName,
                Email,
                IsActive,
                CreatedAt,
                LastLoginAt
            FROM Users
            ORDER BY CreatedAt DESC";

        var users = await _queryDb.Connection.QueryAsync<UserListDto>(sql);

        return Result<List<UserListDto>>.Success(users.ToList());
    }
}
