using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? UserType = null) : IRequest<Result<PaginatedList<UserDto>>>;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string UserType,
    bool IsActive,
    bool IsEmailConfirmed,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
