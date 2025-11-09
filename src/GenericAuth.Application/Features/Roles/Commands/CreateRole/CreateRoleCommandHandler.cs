using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Handler for creating a new system role.
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<CreateRoleCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateRoleCommandResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingRole != null)
        {
            return Result<CreateRoleCommandResponse>.Failure(
                $"A role with the name '{request.Name}' already exists.");
        }

        try
        {
            // Create the role
            var role = Role.Create(request.Name, request.Description);

            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new CreateRoleCommandResponse(
                RoleId: role.Id,
                Name: role.Name,
                Description: role.Description,
                IsActive: role.IsActive,
                Message: "System role created successfully.");

            return Result<CreateRoleCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<CreateRoleCommandResponse>.Failure(ex.Message);
        }
    }
}
