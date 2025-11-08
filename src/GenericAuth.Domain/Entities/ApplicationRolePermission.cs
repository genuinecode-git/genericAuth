namespace GenericAuth.Domain.Entities;

/// <summary>
/// Join entity representing the many-to-many relationship between ApplicationRoles and Permissions.
/// </summary>
public class ApplicationRolePermission
{
    public Guid ApplicationRoleId { get; private set; }
    public ApplicationRole ApplicationRole { get; private set; } = null!;

    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    public DateTime AssignedAt { get; private set; }

    // EF Core constructor
    private ApplicationRolePermission() { }

    public ApplicationRolePermission(Guid applicationRoleId, Guid permissionId)
    {
        ApplicationRoleId = applicationRoleId;
        PermissionId = permissionId;
        AssignedAt = DateTime.UtcNow;
    }
}
