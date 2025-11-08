using GenericAuth.Domain.Common;
using GenericAuth.Domain.Events;
using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.Entities;

/// <summary>
/// Represents a role within a specific application.
/// Unlike global system roles, ApplicationRoles are scoped to their parent application.
/// </summary>
public class ApplicationRole : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; } = null!;

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; } // Default role for new users in this app

    private readonly List<ApplicationRolePermission> _permissions = new();
    public IReadOnlyCollection<ApplicationRolePermission> Permissions => _permissions.AsReadOnly();

    // EF Core constructor
    private ApplicationRole() { }

    private ApplicationRole(Guid applicationId, string name, string description, bool isDefault = false)
    {
        ApplicationId = applicationId;
        Name = name;
        Description = description;
        IsActive = true;
        IsDefault = isDefault;
    }

    /// <summary>
    /// Creates a new application-scoped role.
    /// </summary>
    /// <param name="applicationId">The application this role belongs to</param>
    /// <param name="name">Role name (e.g., "Admin", "User", "Viewer")</param>
    /// <param name="description">Role description</param>
    /// <param name="isDefault">Whether this is the default role for new users</param>
    /// <param name="createdBy">Who created this role</param>
    /// <returns>A new ApplicationRole instance</returns>
    public static ApplicationRole Create(
        Guid applicationId,
        string name,
        string description,
        bool isDefault = false,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Role name cannot be empty.");
        }

        if (name.Length > 100)
        {
            throw new DomainException("Role name cannot exceed 100 characters.");
        }

        var role = new ApplicationRole(applicationId, name.Trim(), description?.Trim() ?? string.Empty, isDefault);
        role.CreatedBy = createdBy;
        role.AddDomainEvent(new ApplicationRoleCreatedEvent(role.Id, applicationId, name));

        return role;
    }

    /// <summary>
    /// Updates the role name and description.
    /// </summary>
    public void Update(string name, string description, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Role name cannot be empty.");
        }

        if (name.Length > 100)
        {
            throw new DomainException("Role name cannot exceed 100 characters.");
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        SetUpdatedInfo(updatedBy);
    }

    /// <summary>
    /// Sets this role as the default role for the application.
    /// </summary>
    public void SetAsDefault(string? updatedBy = null)
    {
        if (IsDefault)
        {
            throw new DomainException("This role is already the default role.");
        }

        IsDefault = true;
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new ApplicationRoleSetAsDefaultEvent(Id, ApplicationId, Name));
    }

    /// <summary>
    /// Removes the default status from this role.
    /// </summary>
    public void RemoveDefaultStatus(string? updatedBy = null)
    {
        if (!IsDefault)
        {
            throw new DomainException("This role is not the default role.");
        }

        IsDefault = false;
        SetUpdatedInfo(updatedBy);
    }

    /// <summary>
    /// Activates the role.
    /// </summary>
    public void Activate(string? updatedBy = null)
    {
        if (IsActive)
        {
            throw new DomainException("Role is already active.");
        }

        IsActive = true;
        SetUpdatedInfo(updatedBy);
    }

    /// <summary>
    /// Deactivates the role.
    /// </summary>
    public void Deactivate(string? updatedBy = null)
    {
        if (!IsActive)
        {
            throw new DomainException("Role is already inactive.");
        }

        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }

    /// <summary>
    /// Adds a permission to this role.
    /// </summary>
    public void AddPermission(Guid permissionId, string? addedBy = null)
    {
        if (_permissions.Any(p => p.PermissionId == permissionId))
        {
            throw new DomainException("Permission is already assigned to this role.");
        }

        var rolePermission = new ApplicationRolePermission(Id, permissionId);
        _permissions.Add(rolePermission);
        SetUpdatedInfo(addedBy);
    }

    /// <summary>
    /// Removes a permission from this role.
    /// </summary>
    public void RemovePermission(Guid permissionId, string? removedBy = null)
    {
        var rolePermission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (rolePermission == null)
        {
            throw new DomainException("Permission is not assigned to this role.");
        }

        _permissions.Remove(rolePermission);
        SetUpdatedInfo(removedBy);
    }
}
