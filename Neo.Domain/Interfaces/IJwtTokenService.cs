namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Provides services for generating JWT tokens for users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the JWT.</param>
    /// <returns>A JWT token string representing the authenticated user.</returns>
    string GenerateToken(User user);
}
