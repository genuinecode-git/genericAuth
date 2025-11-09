using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName) : IRequest<Result<UpdateUserCommandResponse>>;

public record UpdateUserCommandResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Message);
