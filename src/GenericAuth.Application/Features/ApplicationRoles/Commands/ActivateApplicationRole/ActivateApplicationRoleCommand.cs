using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.ActivateApplicationRole;

/// <summary>
/// Command to activate an application role.
/// Only Auth Admins can execute this command.
/// </summary>
public record ActivateApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId) : IRequest<Result<string>>;
