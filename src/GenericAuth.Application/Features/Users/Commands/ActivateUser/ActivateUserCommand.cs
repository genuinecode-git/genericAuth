using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid UserId) : IRequest<Result<string>>;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ActivateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        ActivateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure("User not found");
        }

        user.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("User activated successfully");
    }
}
