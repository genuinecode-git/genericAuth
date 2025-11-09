using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Query to retrieve a paginated list of system roles.
/// System roles are for Auth Admin users only.
/// </summary>
public record GetAllRolesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<Result<PaginatedList<RoleDto>>>;
