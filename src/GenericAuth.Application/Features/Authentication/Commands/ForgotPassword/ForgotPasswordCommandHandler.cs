using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using GenericAuth.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenericAuth.Application.Features.Authentication.Commands.ForgotPassword;

/// <summary>
/// Handler for initiating password reset process.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly IPasswordResetTokenStore _tokenStore;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordCommandHandler> logger,
        IPasswordResetTokenStore tokenStore)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _tokenStore = tokenStore;
    }

    public async Task<Result<string>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var email = Email.Create(request.Email);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            // Don't reveal if email exists or not for security
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return Result<string>.Success(
                    "If the email address exists in our system, a password reset link will be sent.");
            }

            // Generate password reset token
            var resetToken = Guid.NewGuid().ToString("N");

            // Store plain-text token (for testing purposes - no-op in production)
            _tokenStore.StoreToken(request.Email, resetToken);

            // Hash the token before storing in database
            var hashedToken = _passwordHasher.Hash(resetToken);

            // Set token with 1 hour expiration
            user.SetPasswordResetToken(hashedToken, DateTime.UtcNow.AddHours(1));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // In production, send email with reset link
            // For now, log the token (DO NOT do this in production)
            _logger.LogInformation(
                "Password reset token generated for user {Email}. Token: {Token} (This should be sent via email in production)",
                request.Email, resetToken);

            return Result<string>.Success(
                "If the email address exists in our system, a password reset link will be sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for email: {Email}", request.Email);
            return Result<string>.Failure("An error occurred processing your request. Please try again later.");
        }
    }
}
