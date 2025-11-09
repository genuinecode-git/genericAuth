using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoleById;

/// <summary>
/// Query to get detailed information about a specific application role.
/// Only Auth Admins can execute this query.
/// </summary>
public record GetApplicationRoleByIdQuery(
    Guid ApplicationId,
    Guid RoleId) : IRequest<Result<ApplicationRoleDetailDto>>;
