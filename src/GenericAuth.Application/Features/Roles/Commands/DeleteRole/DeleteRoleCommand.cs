using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Command to delete a system role.
/// Cannot delete roles with user assignments.
/// </summary>
public record DeleteRoleCommand(Guid RoleId) : IRequest<Result<string>>;
