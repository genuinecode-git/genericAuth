using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Commands.CreateApplication;

/// <summary>
/// Command to create a new application with its initial roles.
/// Only Auth Admins can execute this command.
/// </summary>
public record CreateApplicationCommand(
    string Name,
    string Code,
    List<CreateApplicationRoleDto> InitialRoles) : IRequest<Result<CreateApplicationCommandResponse>>;

public record CreateApplicationRoleDto(
    string Name,
    string Description,
    bool IsDefault = false);

public record CreateApplicationCommandResponse(
    Guid ApplicationId,
    string Code,
    string ApiKey, // Plain text API key - shown ONLY once
    string Message);
