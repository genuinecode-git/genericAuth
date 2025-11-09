namespace GenericAuth.Application.Features.ApplicationRoles.Queries;

/// <summary>
/// DTO for application role summary information.
/// </summary>
public record ApplicationRoleDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string Description,
    bool IsActive,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO for application role detailed information including permissions and user count.
/// </summary>
public record ApplicationRoleDetailDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string Description,
    bool IsActive,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<PermissionDto> Permissions,
    int UserCount);

/// <summary>
/// DTO for permission information.
/// </summary>
public record PermissionDto(
    Guid Id,
    string Resource,
    string Action,
    string Name,
    string? Description);
