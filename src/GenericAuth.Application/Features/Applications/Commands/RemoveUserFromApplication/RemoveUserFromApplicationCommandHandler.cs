using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Applications.Commands.RemoveUserFromApplication;

/// <summary>
/// Handler for removing a user from an application.
/// </summary>
public class RemoveUserFromApplicationCommandHandler : IRequestHandler<RemoveUserFromApplicationCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RemoveUserFromApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        RemoveUserFromApplicationCommand request,
        CancellationToken cancellationToken)
    {
        // Get the user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<string>.NotFound(
                $"User with ID '{request.UserId}' not found.");
        }

        // Get the application with its user assignments
        var application = await _context.Applications
            .Include(a => a.UserApplications)
            .FirstOrDefaultAsync(a => a.Code.Value == request.ApplicationCode.ToUpperInvariant(), cancellationToken);

        if (application == null)
        {
            return Result<string>.NotFound(
                $"Application with code '{request.ApplicationCode}' not found.");
        }

        // Check if user is assigned to the application before attempting removal
        var isAssigned = application.UserApplications.Any(ua => ua.UserId == user.Id);
        if (!isAssigned)
        {
            return Result<string>.NotFound(
                $"User is not assigned to application '{application.Name}'.");
        }

        // Remove the user from the application
        try
        {
            application.RemoveUser(
                userId: user.Id,
                removedBy: null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"User successfully removed from application '{application.Name}'.");
        }
        catch (DomainException ex)
        {
            // This should not happen due to our check above, but handle it as NotFound
            return Result<string>.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
