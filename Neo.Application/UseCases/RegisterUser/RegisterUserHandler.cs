namespace Neo.Application.UseCases.RegisterUser;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles the registration of new users, checking for existing usernames before creating.
/// </summary>
public sealed class RegisterUserHandler(
    IUserRepository userRepo,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUserHandler> logger
) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    /// <inheritdoc />
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("RegisterUserHandler: Attempting to register user '{Username}' at {Timestamp:O}", request.Username, DateTime.UtcNow);

        // 1. Check if username already exists
        var existing = await userRepo.GetByUsernameAsync(request.Username);
        if (existing is not null)
        {
            logger.LogWarning("RegisterUserHandler: Username '{Username}' already exists at {Timestamp:O}", request.Username, DateTime.UtcNow);
            return new RegisterUserResult(false, null, "Username already exists.");
        }

        // 2. Hash the password securely
        var hash = passwordHasher.HashPassword(request.Password);

        // 3. Create new user entity
        var user = new User
        {
            Username = request.Username,
            PasswordHash = hash,
            Role = request.Role
        };

        // 4. Add user to repository
        var userId = await userRepo.CreateAsync(user);
        logger.LogInformation("RegisterUserHandler: User '{Username}' registered successfully with Id {UserId} at {Timestamp:O}", request.Username, userId, DateTime.UtcNow);

        return new RegisterUserResult(true, userId);
    }
}
