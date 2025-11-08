using GenericAuth.Domain.Common;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    // EF Core constructor
    private Permission() { }

    private Permission(string name, string description, string resource, string action)
    {
        Name = name;
        Description = description;
        Resource = resource;
        Action = action;
        IsActive = true;
    }

    public static Permission Create(string name, string description, string resource, string action)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Permission description cannot be empty.");

        if (string.IsNullOrWhiteSpace(resource))
            throw new DomainException("Resource cannot be empty.");

        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action cannot be empty.");

        return new Permission(name, description, resource, action);
    }

    public void Update(string name, string description, string resource, string action, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Permission name cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Permission description cannot be empty.");

        if (string.IsNullOrWhiteSpace(resource))
            throw new DomainException("Resource cannot be empty.");

        if (string.IsNullOrWhiteSpace(action))
            throw new DomainException("Action cannot be empty.");

        Name = name;
        Description = description;
        Resource = resource;
        Action = action;
        SetUpdatedInfo(updatedBy);
    }

    public void Activate(string? updatedBy = null)
    {
        if (IsActive)
            throw new DomainException("Permission is already active.");

        IsActive = true;
        SetUpdatedInfo(updatedBy);
    }

    public void Deactivate(string? updatedBy = null)
    {
        if (!IsActive)
            throw new DomainException("Permission is already inactive.");

        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }
}
