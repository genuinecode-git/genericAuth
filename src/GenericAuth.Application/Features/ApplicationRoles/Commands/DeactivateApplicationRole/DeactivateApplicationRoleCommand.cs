using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeactivateApplicationRole;

/// <summary>
/// Command to deactivate an application role.
/// Only Auth Admins can execute this command.
/// Cannot deactivate the default role.
/// </summary>
public record DeactivateApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId) : IRequest<Result<string>>;
