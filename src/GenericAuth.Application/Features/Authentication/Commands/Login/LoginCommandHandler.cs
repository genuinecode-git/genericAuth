using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using GenericAuth.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email with appropriate includes based on user type
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.UserApplications)
            .ThenInclude(ua => ua.ApplicationRole)
            .ThenInclude(ar => ar.Permissions)
            .ThenInclude(p => p.Permission)
            .Include(u => u.UserApplications)
            .ThenInclude(ua => ua.Application)
            .FirstOrDefaultAsync(u => u.Email == Email.Create(request.Email), cancellationToken);

        if (user == null)
        {
            return Result<LoginCommandResponse>.Failure("Invalid email or password.");
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.Password.Hash))
        {
            return Result<LoginCommandResponse>.Failure("Invalid email or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<LoginCommandResponse>.Failure("User account is inactive.");
        }

        string token;

        // Generate JWT token based on user type and application context
        if (user.UserType == Domain.Enums.UserType.AuthAdmin)
        {
            // Auth Admin - generate system-level token with roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            token = _jwtTokenGenerator.GenerateToken(user, roles);
        }
        else if (request.ApplicationId.HasValue)
        {
            // Regular user with application context - generate application-scoped token
            var userApplication = user.UserApplications
                .FirstOrDefault(ua => ua.ApplicationId == request.ApplicationId.Value);

            if (userApplication == null)
            {
                return Result<LoginCommandResponse>.Failure("User is not assigned to this application.");
            }

            if (!userApplication.IsActive)
            {
                return Result<LoginCommandResponse>.Failure("User access to this application is inactive.");
            }

            if (!userApplication.ApplicationRole.IsActive)
            {
                return Result<LoginCommandResponse>.Failure("The assigned application role is inactive.");
            }

            var permissions = userApplication.ApplicationRole.Permissions
                .Select(p => p.Permission.Name)
                .ToList();

            token = _jwtTokenGenerator.GenerateApplicationScopedToken(
                user,
                userApplication.ApplicationId,
                userApplication.Application.Code.Value,
                userApplication.ApplicationRole.Name,
                permissions);
        }
        else
        {
            // Regular user without application context - generate basic token with empty roles
            // This allows the user to authenticate but they'll need to specify an application
            // for application-specific operations
            token = _jwtTokenGenerator.GenerateToken(user, new List<string>());
        }

        // Generate and store refresh token
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = Domain.ValueObjects.RefreshToken.Create(refreshTokenString, validityInDays: 7);
        user.AddRefreshToken(refreshToken);

        // Record login
        user.RecordLogin();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return response
        var response = new LoginCommandResponse(
            AccessToken: token,
            RefreshToken: refreshTokenString,
            ExpiresAt: DateTime.UtcNow.AddHours(1), // JWT expiration
            User: new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email.Value));

        return Result<LoginCommandResponse>.Success(response);
    }
}
