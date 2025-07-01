using System;
using System.Security.Cryptography;
using System.Text;
using Neo.Domain.Interfaces;

namespace Neo.Infrastructure.Auth;

/// <summary>
/// Provides password hashing and verification using PBKDF2.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // These parameters can be tuned for security/performance
    private const int SaltSize = 16;     // 128 bit
    private const int KeySize = 32;      // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive a key
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        // Format: {iterations}.{base64 salt}.{base64 hash}
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out var iterations)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        // Derive a key from the input password
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithm,
            hash.Length);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }
}
