using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GenericAuth.Application.Features.ApplicationRoles.Commands.CreateApplicationRole;

/// <summary>
/// Handler for creating a new application role.
/// </summary>
public class CreateApplicationRoleCommandHandler : IRequestHandler<CreateApplicationRoleCommand, Result<CreateApplicationRoleCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateApplicationRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateApplicationRoleCommandResponse>> Handle(
        CreateApplicationRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Load the application with its roles
        var application = await _context.Applications
            .Include(a => a.Roles)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            return Result<CreateApplicationRoleCommandResponse>.Failure(
                $"Application with ID '{request.ApplicationId}' not found.");
        }

        try
        {
            // Create the role through the Application aggregate
            application.CreateRole(
                name: request.Name,
                description: request.Description,
                isDefault: request.IsDefault,
                createdBy: null); // TODO: Get from current user context

            await _context.SaveChangesAsync(cancellationToken);

            // Get the created role
            var createdRole = application.Roles
                .OrderByDescending(r => r.CreatedAt)
                .First();

            var response = new CreateApplicationRoleCommandResponse(
                RoleId: createdRole.Id,
                ApplicationId: createdRole.ApplicationId,
                Name: createdRole.Name,
                Description: createdRole.Description,
                IsDefault: createdRole.IsDefault,
                IsActive: createdRole.IsActive,
                Message: "Application role created successfully.");

            return Result<CreateApplicationRoleCommandResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<CreateApplicationRoleCommandResponse>.Failure(ex.Message);
        }
    }
}
