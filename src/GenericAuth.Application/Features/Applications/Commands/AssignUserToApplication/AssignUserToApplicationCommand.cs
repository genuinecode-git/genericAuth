using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Commands.AssignUserToApplication;

/// <summary>
/// Command to assign a user to an application with a specific role.
/// Only Auth Admins can execute this command.
/// If RoleName is not provided, the default role for the application will be used.
/// </summary>
public record AssignUserToApplicationCommand(
    Guid UserId,
    string ApplicationCode,
    string? RoleName = null) : IRequest<Result<AssignUserToApplicationCommandResponse>>;

public record AssignUserToApplicationCommandResponse(
    Guid UserId,
    Guid ApplicationId,
    Guid RoleId,
    string Message);
