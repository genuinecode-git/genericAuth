using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.SetDefaultApplicationRole;

/// <summary>
/// Command to set an application role as the default role.
/// Only Auth Admins can execute this command.
/// This will remove the default status from any other role in the application.
/// </summary>
public record SetDefaultApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId) : IRequest<Result<string>>;
