using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Commands.AssignRoleToUser;

/// <summary>
/// Command to assign a system role to an Auth Admin user.
/// </summary>
public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId) : IRequest<Result<string>>;
