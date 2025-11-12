using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.UpdateApplicationRole;

/// <summary>
/// Command to update an existing application role.
/// Only Auth Admins can execute this command.
/// </summary>
public record UpdateApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId,
    string Name,
    string Description) : IRequest<Result<UpdateApplicationRoleCommandResponse>>;

public record UpdateApplicationRoleCommandResponse(
    Guid RoleId,
    string Name,
    string Description,
    string Message);
