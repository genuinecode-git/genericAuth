using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// Handler for changing a user's password.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Find user with refresh tokens
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure($"User with ID '{request.UserId}' not found.");
        }

        // Verify current password if not admin reset
        if (!request.IsAdminReset)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return Result<string>.Failure("Current password is required.");
            }

            if (!_passwordHasher.Verify(request.CurrentPassword, user.Password.Hash))
            {
                return Result<string>.Failure("Current password is incorrect.");
            }
        }

        try
        {
            // Hash new password
            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

            // Change password
            user.ChangePassword(newPasswordHash);

            // Revoke all refresh tokens for security
            user.RevokeAllRefreshTokens();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                "Password changed successfully. Please login again with your new password.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
