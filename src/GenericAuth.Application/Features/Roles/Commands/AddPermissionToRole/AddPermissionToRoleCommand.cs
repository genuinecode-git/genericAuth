using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.AddPermissionToRole;

/// <summary>
/// Command to add a permission to a system role.
/// </summary>
public record AddPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId) : IRequest<Result<string>>;
