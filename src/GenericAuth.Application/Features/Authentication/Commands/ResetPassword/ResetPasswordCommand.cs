using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Command to reset a user's password using a reset token.
/// </summary>
public record ResetPasswordCommand(
    string Email,
    string ResetToken,
    string NewPassword) : IRequest<Result<string>>;
