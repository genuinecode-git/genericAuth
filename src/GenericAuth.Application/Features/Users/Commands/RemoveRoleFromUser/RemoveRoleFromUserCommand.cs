using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Commands.RemoveRoleFromUser;

/// <summary>
/// Command to remove a system role from an Auth Admin user.
/// </summary>
public record RemoveRoleFromUserCommand(
    Guid UserId,
    Guid RoleId) : IRequest<Result<string>>;
