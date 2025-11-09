using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Command to create a new system role.
/// System roles are for Auth Admin users only.
/// </summary>
public record CreateRoleCommand(
    string Name,
    string Description) : IRequest<Result<CreateRoleCommandResponse>>;

public record CreateRoleCommandResponse(
    Guid RoleId,
    string Name,
    string Description,
    bool IsActive,
    string Message);
