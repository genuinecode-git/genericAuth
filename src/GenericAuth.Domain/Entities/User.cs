using GenericAuth.Domain.Common;
using GenericAuth.Domain.Enums;
using GenericAuth.Domain.Events;
using GenericAuth.Domain.Exceptions;
using GenericAuth.Domain.ValueObjects;

namespace GenericAuth.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    /// <summary>
    /// Type of user: Regular (application-scoped) or AuthAdmin (system-level).
    /// </summary>
    public UserType UserType { get; private set; }

    // System-level roles (for Auth Admins)
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Application-scoped assignments (for Regular users)
    private readonly List<UserApplication> _userApplications = new();
    public IReadOnlyCollection<UserApplication> UserApplications => _userApplications.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // EF Core constructor
    private User() { }

    private User(string firstName, string lastName, Email email, Password password, UserType userType)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        UserType = userType;
        IsActive = true;
        IsEmailConfirmed = false;
        EmailConfirmationToken = Guid.NewGuid().ToString("N");
    }

    public static User Create(string firstName, string lastName, string email, string passwordHash, UserType userType = UserType.RegularUser)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");

        var userEmail = Email.Create(email);
        var userPassword = Password.Create(passwordHash);

        var user = new User(firstName, lastName, userEmail, userPassword, userType);
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value));
        return user;
    }

    /// <summary>
    /// Creates an Auth Admin user with system-level privileges.
    /// </summary>
    public static User CreateAuthAdmin(string firstName, string lastName, string email, string passwordHash)
    {
        return Create(firstName, lastName, email, passwordHash, UserType.AuthAdmin);
    }

    public void UpdateProfile(string firstName, string lastName, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");

        FirstName = firstName;
        LastName = lastName;
        SetUpdatedInfo(updatedBy);
    }

    public void ChangePassword(string newPasswordHash, string? updatedBy = null)
    {
        Password = Password.Create(newPasswordHash);
        SetUpdatedInfo(updatedBy);
        AddDomainEvent(new PasswordChangedEvent(Id, Email.Value));
    }

    public void ConfirmEmail()
    {
        if (IsEmailConfirmed)
            throw new DomainException("Email is already confirmed.");

        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        SetUpdatedInfo();
    }

    public void Activate(string? updatedBy = null)
    {
        if (IsActive)
            throw new DomainException("User is already active.");

        IsActive = true;
        SetUpdatedInfo(updatedBy);
    }

    public void Deactivate(string? updatedBy = null)
    {
        if (!IsActive)
            throw new DomainException("User is already inactive.");

        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id, Email.Value));
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RevokeRefreshToken(string token, string? replacedByToken = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        if (refreshToken == null)
            throw new DomainException("Refresh token not found.");

        refreshToken.Revoke(replacedByToken);
        SetUpdatedInfo();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }
        SetUpdatedInfo();
    }

    public RefreshToken? GetActiveRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(rt => rt.Token == token && rt.IsActive);
    }

    public void SetPasswordResetToken(string hashedToken, DateTime expiresAt)
    {
        PasswordResetToken = hashedToken;
        PasswordResetTokenExpiresAt = expiresAt;
        SetUpdatedInfo();
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        SetUpdatedInfo();
    }

    public bool ValidatePasswordResetToken(string hashedToken)
    {
        if (string.IsNullOrEmpty(PasswordResetToken))
            return false;

        if (PasswordResetTokenExpiresAt == null || PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return false;

        return PasswordResetToken == hashedToken;
    }

    public void AddRole(UserRole userRole)
    {
        if (_userRoles.Any(ur => ur.RoleId == userRole.RoleId))
            throw new DomainException("User already has this role.");

        _userRoles.Add(userRole);
        SetUpdatedInfo();
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            throw new DomainException("User does not have this role.");

        _userRoles.Remove(userRole);
        SetUpdatedInfo();
    }

    /// <summary>
    /// Checks if user is an Auth Admin with system-level privileges.
    /// </summary>
    public bool IsAuthAdmin() => UserType == UserType.AuthAdmin;

    /// <summary>
    /// Checks if user has access to a specific application.
    /// </summary>
    public bool HasAccessToApplication(Guid applicationId)
    {
        if (IsAuthAdmin())
            return true; // Auth admins have access to all applications

        return _userApplications.Any(ua => ua.ApplicationId == applicationId && ua.IsActive);
    }

    /// <summary>
    /// Gets the user's role in a specific application.
    /// </summary>
    public UserApplication GetApplicationAssignment(Guid applicationId)
    {
        var assignment = _userApplications.FirstOrDefault(ua => ua.ApplicationId == applicationId);
        if (assignment == null)
        {
            throw new DomainException("User is not assigned to this application.");
        }

        return assignment;
    }

    /// <summary>
    /// Assigns this user to an application with a specific role.
    /// This method is used internally; external assignment should go through the Application aggregate.
    /// </summary>
    internal void AddApplicationAssignment(UserApplication userApplication)
    {
        if (_userApplications.Any(ua => ua.ApplicationId == userApplication.ApplicationId))
        {
            throw new DomainException("User is already assigned to this application.");
        }

        _userApplications.Add(userApplication);
    }

    /// <summary>
    /// Removes an application assignment from this user.
    /// This method is used internally; external removal should go through the Application aggregate.
    /// </summary>
    internal void RemoveApplicationAssignment(Guid applicationId)
    {
        var assignment = _userApplications.FirstOrDefault(ua => ua.ApplicationId == applicationId);
        if (assignment == null)
        {
            throw new DomainException("User is not assigned to this application.");
        }

        _userApplications.Remove(assignment);
    }

    /// <summary>
    /// Records a login for a specific application.
    /// </summary>
    public void RecordLogin(Guid? applicationId = null)
    {
        LastLoginAt = DateTime.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id, Email.Value));

        // Record application-specific access
        if (applicationId.HasValue && !IsAuthAdmin())
        {
            var assignment = _userApplications.FirstOrDefault(ua => ua.ApplicationId == applicationId.Value);
            assignment?.RecordAccess();
        }
    }
}
