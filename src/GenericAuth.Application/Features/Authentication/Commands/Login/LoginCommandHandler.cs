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
        // Find user by email
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
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

        // Generate JWT token
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        // Generate and store refresh token
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(refreshTokenString, validityInDays: 7);
        user.AddRefreshToken(refreshToken);

        // Record login
        user.RecordLogin();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return response
        var response = new LoginCommandResponse(
            Token: token,
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
