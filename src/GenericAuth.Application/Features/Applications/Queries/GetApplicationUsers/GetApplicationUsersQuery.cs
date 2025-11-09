using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Queries.GetApplicationUsers;

/// <summary>
/// Query to get all users assigned to a specific application with their roles.
/// Only Auth Admins can execute this query.
/// </summary>
public record GetApplicationUsersQuery(
    string ApplicationCode,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<Result<PaginatedList<ApplicationUserDto>>>;

/// <summary>
/// DTO for application's user assignment information.
/// </summary>
public record ApplicationUserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string UserType,
    Guid RoleId,
    string RoleName,
    bool RoleIsActive,
    bool RoleIsDefault,
    DateTime AssignedAt,
    bool IsActive);
