using GenericAuth.Application.Common.Interfaces;
using GenericAuth.Application.Common.Models;
using GenericAuth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = GenericAuth.Domain.Entities.Application;

namespace GenericAuth.Application.Features.Applications.Commands.CreateApplication;

public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Result<CreateApplicationCommandResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateApplicationCommandResponse>> Handle(
        CreateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        // Check if application code already exists
        var existingApp = await _context.Applications
            .FirstOrDefaultAsync(a => a.Code.Value == request.Code.ToUpperInvariant(), cancellationToken);

        if (existingApp != null)
        {
            return Result<CreateApplicationCommandResponse>.Failure(
                $"Application with code '{request.Code}' already exists.");
        }

        // Create the application (returns plain API key)
        var (application, plainApiKey) = ApplicationEntity.Create(
            name: request.Name,
            code: request.Code,
            createdBy: null); // TODO: Get from current user context

        // Add initial roles to the application
        foreach (var roleDto in request.InitialRoles)
        {
            application.CreateRole(
                name: roleDto.Name,
                description: roleDto.Description,
                isDefault: roleDto.IsDefault,
                createdBy: null); // TODO: Get from current user context
        }

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateApplicationCommandResponse(
            ApplicationId: application.Id,
            Code: application.Code.Value,
            ApiKey: plainApiKey, // IMPORTANT: This is the only time the plain API key is returned
            Message: "Application created successfully. Please store the API key securely - it will not be shown again.");

        return Result<CreateApplicationCommandResponse>.Success(response);
    }
}
