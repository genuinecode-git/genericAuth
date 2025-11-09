using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Command to change a user's password.
/// Supports both user-initiated password change (with current password verification)
/// and admin-initiated password reset (without current password verification).
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    string? CurrentPassword,
    string NewPassword,
    bool IsAdminReset = false) : IRequest<Result<string>>;
