namespace Neo.Infrastructure.Auth;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides utilities for hashing and verifying user passwords.
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Hashes the provided plain-text password using SHA-256.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A base64-encoded hash string.</returns>
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Verifies a password against a previously computed hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The stored password hash.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
    public static bool VerifyPassword(string password, string hash)
        => HashPassword(password) == hash;
}
