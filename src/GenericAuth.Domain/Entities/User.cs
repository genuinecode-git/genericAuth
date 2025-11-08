using GenericAuth.Domain.Common;
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

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // EF Core constructor
    private User() { }

    private User(string firstName, string lastName, Email email, Password password)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        IsActive = true;
        IsEmailConfirmed = false;
        EmailConfirmationToken = Guid.NewGuid().ToString("N");
    }

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty.");

        var userEmail = Email.Create(email);
        var userPassword = Password.Create(passwordHash);

        var user = new User(firstName, lastName, userEmail, userPassword);
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value));
        return user;
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
}
