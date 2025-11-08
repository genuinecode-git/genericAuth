namespace GenericAuth.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    public DateTime AssignedAt { get; private set; }

    // EF Core constructor
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        AssignedAt = DateTime.UtcNow;
    }
}
