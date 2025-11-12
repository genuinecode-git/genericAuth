using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.DeactivateRole;

/// <summary>
/// Handler for deactivating a system role.
/// </summary>
public class DeactivateRoleCommandHandler : IRequestHandler<DeactivateRoleCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public DeactivateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(
        DeactivateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result<string>.Failure(
                $"Role with ID '{request.RoleId}' not found.");
        }

        try
        {
            role.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success($"Role '{role.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }
}
