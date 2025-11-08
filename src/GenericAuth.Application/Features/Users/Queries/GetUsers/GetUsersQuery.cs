using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery() : IRequest<Result<List<UserListDto>>>;

public record UserListDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);
