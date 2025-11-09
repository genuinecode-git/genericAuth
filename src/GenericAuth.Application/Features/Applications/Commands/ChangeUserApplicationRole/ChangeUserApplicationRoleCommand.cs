using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Commands.ChangeUserApplicationRole;

/// <summary>
/// Command to change a user's role within an application.
/// Only Auth Admins can execute this command.
/// </summary>
public record ChangeUserApplicationRoleCommand(
    Guid UserId,
    string ApplicationCode,
    Guid NewApplicationRoleId) : IRequest<Result<ChangeUserApplicationRoleCommandResponse>>;

public record ChangeUserApplicationRoleCommandResponse(
    Guid UserId,
    Guid ApplicationId,
    Guid NewRoleId,
    string NewRoleName,
    string Message);
