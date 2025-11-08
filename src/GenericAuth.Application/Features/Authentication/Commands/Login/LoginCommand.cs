using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginCommandResponse>>;

public record LoginCommandResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);
