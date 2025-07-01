namespace Neo.Domain.Enums;

/// <summary>
/// Enumerates the available user roles in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Represents an undefined or unknown user role.
    /// </summary>
    None = -1,
    /// <summary>
    /// Represents a regular user.
    /// </summary>
    User = 0,

    /// <summary>
    /// Represents a moderator.
    /// </summary>
    Moderator = 1,

}
