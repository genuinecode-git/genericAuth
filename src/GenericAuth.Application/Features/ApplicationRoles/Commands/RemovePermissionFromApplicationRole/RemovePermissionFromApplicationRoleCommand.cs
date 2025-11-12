using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.RemovePermissionFromApplicationRole;

/// <summary>
/// Command to remove a permission from an application role.
/// Only Auth Admins can execute this command.
/// </summary>
public record RemovePermissionFromApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId,
    Guid PermissionId) : IRequest<Result<string>>;
