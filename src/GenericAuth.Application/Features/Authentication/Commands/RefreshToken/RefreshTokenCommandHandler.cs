using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing an access token.
/// </summary>
public class RefreshTokenCommandHandler : MediatR.IRequestHandler<RefreshTokenCommand, Result<RefreshTokenCommandResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenCommandResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Find user with matching refresh token
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserApplications)
                .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken),
                cancellationToken);

        if (user == null)
        {
            return Result<RefreshTokenCommandResponse>.Failure("Invalid refresh token.");
        }

        // Get the refresh token
        var refreshToken = user.GetActiveRefreshToken(request.RefreshToken);

        if (refreshToken == null)
        {
            return Result<RefreshTokenCommandResponse>.Failure(
                "Refresh token is invalid, expired, or has been revoked.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<RefreshTokenCommandResponse>.Failure("User account is inactive.");
        }

        try
        {
            // Generate new JWT token
            string newToken;

            if (user.IsAuthAdmin())
            {
                // For Auth Admins: use system-level roles
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                newToken = _jwtTokenGenerator.GenerateToken(user, roles);
            }
            else
            {
                // For Regular users: check if they have an active application assignment
                // Note: In a real scenario, you might need to pass application context
                // For now, we'll generate a basic token
                var roles = new List<string> { "User" };
                newToken = _jwtTokenGenerator.GenerateToken(user, roles);
            }

            // Generate new refresh token
            var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
            var newRefreshToken = Domain.ValueObjects.RefreshToken.Create(newRefreshTokenString, validityInDays: 7);

            // Revoke old refresh token and add new one
            user.RevokeRefreshToken(request.RefreshToken, newRefreshTokenString);
            user.AddRefreshToken(newRefreshToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return response
            var response = new RefreshTokenCommandResponse(
                Token: newToken,
                RefreshToken: newRefreshTokenString,
                ExpiresAt: DateTime.UtcNow.AddHours(1), // JWT expiration
                User: new UserDto(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email.Value));

            return Result<RefreshTokenCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<RefreshTokenCommandResponse>.Failure(ex.Message);
        }
    }
}
