using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Queries.GetUserApplications;

/// <summary>
/// Query to get all applications assigned to a specific user.
/// Only Auth Admins can execute this query.
/// </summary>
public record GetUserApplicationsQuery(Guid UserId) : IRequest<Result<List<UserApplicationDto>>>;

/// <summary>
/// DTO for user's application assignment information.
/// </summary>
public record UserApplicationDto(
    Guid ApplicationId,
    string ApplicationCode,
    string ApplicationName,
    bool ApplicationIsActive,
    Guid RoleId,
    string RoleName,
    bool RoleIsActive,
    bool RoleIsDefault,
    DateTime AssignedAt,
    bool IsActive);
