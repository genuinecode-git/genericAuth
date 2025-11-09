using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Commands.RemoveUserFromApplication;

/// <summary>
/// Command to remove a user from an application.
/// Only Auth Admins can execute this command.
/// </summary>
public record RemoveUserFromApplicationCommand(
    Guid UserId,
    string ApplicationCode) : IRequest<Result<string>>;
