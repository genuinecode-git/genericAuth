using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest<Result<string>>;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        DeactivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure("User not found");
        }

        user.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("User deactivated successfully");
    }
}
