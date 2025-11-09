using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process.
/// Generates a password reset token for the user.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
