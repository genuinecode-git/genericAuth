using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Authentication.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<RegisterCommandResponse>>;

public record RegisterCommandResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string UserType,
    string Message);
