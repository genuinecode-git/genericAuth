using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using GenericAuth.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Authentication.Commands.ResetPassword;

/// <summary>
/// Handler for resetting a user's password.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var email = Email.Create(request.Email);
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure("Invalid email or reset token.");
        }

        // Verify the provided token against the stored hash
        if (user.PasswordResetToken == null ||
            !_passwordHasher.Verify(request.ResetToken, user.PasswordResetToken))
        {
            return Result<string>.Failure(
                "Invalid or expired reset token. Please request a new password reset.");
        }

        // Check expiration separately for clarity
        if (user.PasswordResetTokenExpiresAt == null ||
            user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return Result<string>.Failure(
                "Invalid or expired reset token. Please request a new password reset.");
        }

        try
        {
            // Hash new password
            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

            // Update password
            user.ChangePassword(newPasswordHash);

            // Clear reset token
            user.ClearPasswordResetToken();

            // Revoke all refresh tokens for security
            user.RevokeAllRefreshTokens();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                "Password reset successfully. Please login with your new password.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
