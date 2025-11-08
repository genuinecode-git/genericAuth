using GenericAuth.Domain.Common;
using GenericAuth.Domain.Events;
using GenericAuth.Domain.Exceptions;
using GenericAuth.Domain.ValueObjects;

namespace GenericAuth.Domain.Entities;

/// <summary>
/// Represents an external application that integrates with the authentication system.
/// Applications can have multiple users with application-specific roles.
/// </summary>
public class Application : BaseEntity
{
    public string Name { get; private set; }
    public ApplicationCode Code { get; private set; }
    public ApiKey ApiKey { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ApplicationRole> _roles = new();
    public IReadOnlyCollection<ApplicationRole> Roles => _roles.AsReadOnly();

    private readonly List<UserApplication> _userApplications = new();
    public IReadOnlyCollection<UserApplication> UserApplications => _userApplications.AsReadOnly();

    // EF Core constructor
    private Application() { }

    private Application(string name, ApplicationCode code, ApiKey apiKey)
    {
        Name = name;
        Code = code;
        ApiKey = apiKey;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new application with a cryptographically secure API key.
    /// </summary>
    /// <param name="name">Application name</param>
    /// <param name="code">Unique application code</param>
    /// <param name="createdBy">Who created the application (Auth Admin)</param>
    /// <returns>Tuple containing the Application and the plain text API key (for one-time display)</returns>
    public static (Application application, string plainApiKey) Create(
        string name,
        string code,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Application name cannot be empty.");
        }

        if (name.Length > 200)
        {
            throw new DomainException("Application name cannot exceed 200 characters.");
        }

        var appCode = ApplicationCode.Create(code);
        var (apiKey, plainKey) = ApiKey.Generate();

        var application = new Application(name.Trim(), appCode, apiKey);
        application.CreatedBy = createdBy;
        application.AddDomainEvent(new ApplicationCreatedEvent(application.Id, appCode.Value));

        return (application, plainKey);
    }

    /// <summary>
    /// Updates the application name.
    /// </summary>
    public void UpdateName(string name, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Application name cannot be empty.");
        }

        if (name.Length > 200)
        {
            throw new DomainException("Application name cannot exceed 200 characters.");
        }

        Name = name.Trim();
        SetUpdatedInfo(updatedBy);
    }

    /// <summary>
    /// Regenerates the API key for security purposes.
    /// </summary>
    /// <returns>The new plain text API key (for one-time display)</returns>
    public string RegenerateApiKey(string? updatedBy = null)
    {
        var oldKeyHash = ApiKey.HashedValue;
        var (newApiKey, plainKey) = ApiKey.Generate();

        ApiKey = newApiKey;
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new ApiKeyRotatedEvent(Id, Code.Value, oldKeyHash));

        return plainKey;
    }

    /// <summary>
    /// Validates a plain text API key against the stored hash.
    /// </summary>
    public bool ValidateApiKey(string plainApiKey)
    {
        return IsActive && ApiKey.Validate(plainApiKey);
    }

    /// <summary>
    /// Activates the application.
    /// </summary>
    public void Activate(string? updatedBy = null)
    {
        if (IsActive)
        {
            throw new DomainException("Application is already active.");
        }

        IsActive = true;
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new ApplicationActivatedEvent(Id, Code.Value));
    }

    /// <summary>
    /// Deactivates the application, preventing all authentication.
    /// </summary>
    public void Deactivate(string? updatedBy = null)
    {
        if (!IsActive)
        {
            throw new DomainException("Application is already inactive.");
        }

        IsActive = false;
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new ApplicationDeactivatedEvent(Id, Code.Value));
    }

    /// <summary>
    /// Creates a new role for this application.
    /// </summary>
    public void CreateRole(string name, string description, bool isDefault = false, string? createdBy = null)
    {
        if (_roles.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"Role '{name}' already exists in this application.");
        }

        // If this is set as default, remove default status from other roles
        if (isDefault)
        {
            var currentDefault = _roles.FirstOrDefault(r => r.IsDefault);
            if (currentDefault != null)
            {
                currentDefault.RemoveDefaultStatus(createdBy);
            }
        }

        var role = ApplicationRole.Create(Id, name, description, isDefault, createdBy);
        _roles.Add(role);
        SetUpdatedInfo(createdBy);
    }

    /// <summary>
    /// Gets a role by ID within this application.
    /// </summary>
    public ApplicationRole GetRole(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.Id == roleId);
        if (role == null)
        {
            throw new DomainException($"Role with ID '{roleId}' not found in this application.");
        }

        return role;
    }

    /// <summary>
    /// Gets a role by name within this application.
    /// </summary>
    public ApplicationRole GetRoleByName(string roleName)
    {
        var role = _roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role == null)
        {
            throw new DomainException($"Role '{roleName}' not found in this application.");
        }

        return role;
    }

    /// <summary>
    /// Assigns a user to this application with a specific role.
    /// </summary>
    public void AssignUser(Guid userId, Guid applicationRoleId, string? assignedBy = null)
    {
        // Verify role belongs to this application
        var role = GetRole(applicationRoleId);

        if (!role.IsActive)
        {
            throw new DomainException($"Cannot assign user to inactive role '{role.Name}'.");
        }

        if (_userApplications.Any(ua => ua.UserId == userId))
        {
            throw new DomainException("User is already assigned to this application.");
        }

        var userApplication = new UserApplication(userId, Id, applicationRoleId, assignedBy);
        _userApplications.Add(userApplication);
        SetUpdatedInfo(assignedBy);
        AddDomainEvent(new UserAssignedToApplicationEvent(Id, userId, applicationRoleId));
    }

    /// <summary>
    /// Changes a user's role within this application.
    /// </summary>
    public void ChangeUserRole(Guid userId, Guid newRoleId, string? updatedBy = null)
    {
        var userApplication = _userApplications.FirstOrDefault(ua => ua.UserId == userId);
        if (userApplication == null)
        {
            throw new DomainException("User is not assigned to this application.");
        }

        // Verify new role belongs to this application
        var newRole = GetRole(newRoleId);

        if (!newRole.IsActive)
        {
            throw new DomainException($"Cannot assign user to inactive role '{newRole.Name}'.");
        }

        userApplication.ChangeRole(newRoleId, updatedBy);
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new UserRoleChangedInApplicationEvent(Id, userId, newRoleId));
    }

    /// <summary>
    /// Removes a user from this application.
    /// </summary>
    public void RemoveUser(Guid userId, string? removedBy = null)
    {
        var userApplication = _userApplications.FirstOrDefault(ua => ua.UserId == userId);
        if (userApplication == null)
        {
            throw new DomainException("User is not assigned to this application.");
        }

        _userApplications.Remove(userApplication);
        SetUpdatedInfo(removedBy);
        AddDomainEvent(new UserRemovedFromApplicationEvent(Id, userId));
    }

    /// <summary>
    /// Verifies if a user has access to this application.
    /// </summary>
    public bool HasUser(Guid userId)
    {
        return _userApplications.Any(ua => ua.UserId == userId && ua.IsActive);
    }

    /// <summary>
    /// Gets a user's application assignment including their role.
    /// </summary>
    public UserApplication GetUserApplication(Guid userId)
    {
        var userApp = _userApplications.FirstOrDefault(ua => ua.UserId == userId);
        if (userApp == null)
        {
            throw new DomainException("User is not assigned to this application.");
        }

        return userApp;
    }
}
