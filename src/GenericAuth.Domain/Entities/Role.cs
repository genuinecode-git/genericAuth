using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // EF Core constructor
    private Role() { }

    private Role(string name, string description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    public static Role Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Role description cannot be empty.");

        return new Role(name, description);
    }

    public void Update(string name, string description, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Role description cannot be empty.");

        Name = name;
        Description = description;
        SetUpdatedInfo(updatedBy);
    }

    public void Activate(string? updatedBy = null)
    {
        if (IsActive)
            throw new DomainException("Role is already active.");

        IsActive = true;
        SetUpdatedInfo(updatedBy);
    }

    public void Deactivate(string? updatedBy = null)
    {
        if (!IsActive)
            throw new DomainException("Role is already inactive.");

        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }

    public void AddPermission(RolePermission rolePermission)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == rolePermission.PermissionId))
            throw new DomainException("Role already has this permission.");

        _rolePermissions.Add(rolePermission);
        SetUpdatedInfo();
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission == null)
            throw new DomainException("Role does not have this permission.");

        _rolePermissions.Remove(rolePermission);
        SetUpdatedInfo();
    }
}
