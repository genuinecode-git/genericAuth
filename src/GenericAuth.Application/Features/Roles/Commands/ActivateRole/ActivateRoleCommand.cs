using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.ActivateRole;

/// <summary>
/// Command to activate a system role.
/// </summary>
public record ActivateRoleCommand(Guid RoleId) : IRequest<Result<string>>;
