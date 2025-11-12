using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.RemovePermissionFromRole;

/// <summary>
/// Command to remove a permission from a system role.
/// </summary>
public record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId) : IRequest<Result<string>>;
