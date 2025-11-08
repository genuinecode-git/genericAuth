using GenericAuth.Domain.Exceptions;

namespace GenericAuth.Domain.Entities;

/// <summary>
/// Join entity representing a user's assignment to an application with a specific role.
/// Tracks the many-to-many relationship: User ↔ Application with role context.
/// </summary>
public class UserApplication
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; } = null!;

    /// <summary>
    /// The role this user has within this application.
    /// </summary>
    public Guid ApplicationRoleId { get; private set; }
    public ApplicationRole ApplicationRole { get; private set; } = null!;

    public DateTime AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private UserApplication() { }

    public UserApplication(Guid userId, Guid applicationId, Guid applicationRoleId, string? assignedBy = null)
    {
        UserId = userId;
        ApplicationId = applicationId;
        ApplicationRoleId = applicationRoleId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
        IsActive = true;
    }

    /// <summary>
    /// Records that the user accessed this application.
    /// </summary>
    public void RecordAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the user's role within this application.
    /// </summary>
    public void ChangeRole(Guid newApplicationRoleId, string? updatedBy = null)
    {
        if (ApplicationRoleId == newApplicationRoleId)
        {
            throw new DomainException("User already has this role in the application.");
        }

        ApplicationRoleId = newApplicationRoleId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user's access to this application.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            throw new DomainException("User application access is already active.");
        }

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user's access to this application without removing the assignment.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("User application access is already inactive.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
