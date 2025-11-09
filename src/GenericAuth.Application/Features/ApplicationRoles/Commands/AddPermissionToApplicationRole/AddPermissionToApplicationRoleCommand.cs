using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.AddPermissionToApplicationRole;

/// <summary>
/// Command to add a permission to an application role.
/// Only Auth Admins can execute this command.
/// </summary>
public record AddPermissionToApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId,
    Guid PermissionId) : IRequest<Result<string>>;
