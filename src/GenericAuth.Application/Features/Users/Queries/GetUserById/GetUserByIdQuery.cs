using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDetailDto>>;

public record UserDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string UserType,
    bool IsActive,
    bool IsEmailConfirmed,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<UserApplicationDto> Applications);

public record UserApplicationDto(
    Guid ApplicationId,
    string ApplicationCode,
    string ApplicationName,
    string RoleName,
    bool IsActive,
    DateTime AssignedAt,
    DateTime? LastAccessedAt);
