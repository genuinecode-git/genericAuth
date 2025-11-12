namespace GenericAuth.Application.Features.Roles.Queries;

/// <summary>
/// DTO for system role summary information.
/// System roles are for Auth Admin users only.
/// </summary>
public record RoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    int PermissionCount,
    int UserCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO for system role detailed information including permissions and assigned users.
/// </summary>
public record RoleDetailDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<PermissionDto> Permissions,
    List<RoleUserDto> Users);

/// <summary>
/// DTO for permission information within a role.
/// </summary>
public record PermissionDto(
    Guid Id,
    string Name,
    string Resource,
    string Action,
    string? Description);

/// <summary>
/// DTO for user information within a role.
/// </summary>
public record RoleUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime AssignedAt);
