using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Query to retrieve detailed information about a specific system role.
/// </summary>
public record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDetailDto>>;
