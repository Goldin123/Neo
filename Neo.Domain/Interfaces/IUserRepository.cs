namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Defines contract for user data access.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The <see cref="User"/> if found; otherwise, <c>null</c>.</returns>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>The <see cref="User"/> if found; otherwise, <c>null</c>.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user and returns the new user's unique identifier.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The unique identifier of the newly created user.</returns>
    Task<int> CreateAsync(User user);

    /// <summary>
    /// Checks if a username already exists in the system.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns><c>true</c> if the username exists; otherwise, <c>false</c>.</returns>
    Task<bool> UsernameExistsAsync(string username);
}
