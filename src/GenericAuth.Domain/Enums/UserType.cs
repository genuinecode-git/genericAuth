namespace GenericAuth.Domain.Enums;

/// <summary>
/// Defines the type of user in the system.
/// </summary>
public enum UserType
{
    /// <summary>
    /// Regular user with application-scoped access.
    /// Must authenticate with application context and assigned roles.
    /// </summary>
    RegularUser = 0,

    /// <summary>
    /// System administrator with super admin privileges.
    /// Can register applications, assign users, and manage system-wide settings.
    /// </summary>
    AuthAdmin = 1
}
