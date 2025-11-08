using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;

/// <summary>
/// Command to assign a user to an application with a specific role.
/// Only Auth Admins can execute this command.
/// </summary>
public record AssignUserToApplicationCommand(
    Guid UserId,
    string ApplicationCode,
    string RoleName) : IRequest<Result<AssignUserToApplicationCommandResponse>>;

public record AssignUserToApplicationCommandResponse(
    Guid UserId,
    Guid ApplicationId,
    Guid RoleId,
    string Message);
