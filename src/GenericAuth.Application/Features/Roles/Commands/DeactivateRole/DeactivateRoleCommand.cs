using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.DeactivateRole;

/// <summary>
/// Command to deactivate a system role.
/// </summary>
public record DeactivateRoleCommand(Guid RoleId) : IRequest<Result<string>>;
