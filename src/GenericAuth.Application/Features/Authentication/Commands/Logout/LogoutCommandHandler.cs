using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Authentication.Commands.Logout;

/// <summary>
/// Handler for logging out a user.
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Find user with refresh tokens
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure($"User with ID '{request.UserId}' not found.");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                // Revoke specific refresh token
                user.RevokeRefreshToken(request.RefreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<string>.Success("Logged out successfully.");
            }
            else
            {
                // Revoke all refresh tokens
                user.RevokeAllRefreshTokens();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<string>.Success("Logged out from all devices successfully.");
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
