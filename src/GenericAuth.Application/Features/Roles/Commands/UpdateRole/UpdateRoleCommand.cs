using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Command to update an existing system role.
/// </summary>
public record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string Description) : IRequest<Result<UpdateRoleCommandResponse>>;

public record UpdateRoleCommandResponse(
    Guid RoleId,
    string Name,
    string Description,
    bool IsActive,
    string Message);
