using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.CreateApplicationRole;

/// <summary>
/// Command to create a new role for an application.
/// Only Auth Admins can execute this command.
/// </summary>
public record CreateApplicationRoleCommand(
    Guid ApplicationId,
    string Name,
    string Description,
    bool IsDefault = false) : IRequest<Result<CreateApplicationRoleCommandResponse>>;

public record CreateApplicationRoleCommandResponse(
    Guid RoleId,
    Guid ApplicationId,
    string Name,
    string Description,
    bool IsDefault,
    bool IsActive,
    string Message);
