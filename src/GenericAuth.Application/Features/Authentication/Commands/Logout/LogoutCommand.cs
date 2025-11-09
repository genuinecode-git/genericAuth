using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Command to logout a user by revoking refresh tokens.
/// If RefreshToken is provided, only that token is revoked.
/// If RefreshToken is null, all refresh tokens for the user are revoked.
/// </summary>
public record LogoutCommand(
    Guid UserId,
    string? RefreshToken = null) : IRequest<Result<string>>;
