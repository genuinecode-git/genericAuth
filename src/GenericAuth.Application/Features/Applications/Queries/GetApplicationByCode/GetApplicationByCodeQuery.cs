using GenericAuth.Application.Common.Models;
using MediatR;

namespace GenericAuth.Application.Features.Applications.Queries.GetApplicationByCode;

/// <summary>
/// Query to get an application by its unique code.
/// Critical for authentication - validates application before user login.
/// </summary>
public record GetApplicationByCodeQuery(string ApplicationCode) : IRequest<Result<ApplicationDto>>;

public record ApplicationDto(
    Guid Id,
    string Name,
    string Code,
    bool IsActive,
    DateTime CreatedAt);

public record ApplicationRoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    bool IsDefault);
