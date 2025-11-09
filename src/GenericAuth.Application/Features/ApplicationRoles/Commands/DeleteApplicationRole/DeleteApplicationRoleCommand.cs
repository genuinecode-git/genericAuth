using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.DeleteApplicationRole;

/// <summary>
/// Command to delete an application role.
/// Only Auth Admins can execute this command.
/// Cannot delete default roles or roles with user assignments.
/// </summary>
public record DeleteApplicationRoleCommand(
    Guid ApplicationId,
    Guid RoleId) : IRequest<Result<string>>;
