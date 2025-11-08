using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using GenericAuth.Domain.Interfaces;
using GenericAuth.Domain.Services;
using GenericAuth.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterCommandResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterCommandResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user with email already exists
        var emailValue = Email.Create(request.Email);
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == emailValue, cancellationToken);

        if (existingUser != null)
        {
            return Result<RegisterCommandResponse>.Failure("User with this email already exists.");
        }

        // Hash password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create user
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash);

        // Add to database
        await _context.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return response
        var response = new RegisterCommandResponse(
            user.Id,
            user.Email.Value,
            "User registered successfully. Please confirm your email.");

        return Result<RegisterCommandResponse>.Success(response);
    }
}
