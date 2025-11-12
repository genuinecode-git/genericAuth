using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UpdateUserCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateUserCommandResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<UpdateUserCommandResponse>.Failure("User not found");
        }

        user.UpdateProfile(request.FirstName, request.LastName);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateUserCommandResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            "User updated successfully");

        return Result<UpdateUserCommandResponse>.Success(response);
    }
}
