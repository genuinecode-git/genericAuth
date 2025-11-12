using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Queries.GetApplicationRoles;

/// <summary>
/// Query to get a paginated list of roles for a specific application.
/// Only Auth Admins can execute this query.
/// </summary>
public record GetApplicationRolesQuery(
    Guid ApplicationId,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<Result<PaginatedList<ApplicationRoleDto>>>;
