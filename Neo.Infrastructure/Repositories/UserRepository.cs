namespace Neo.Infrastructure.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Data;
using System.Data;

/// <summary>
/// Provides data access functionality for <see cref="User"/> entities.
/// </summary>
public sealed class UserRepository(DbContext dbContext, ILogger<UserRepository> logger) : IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>(
                "spUser_GetById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by Id {Id} at {Timestamp:O}", id, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<User>(
                "spUser_GetByUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by username {Username} at {Timestamp:O}", username, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(User user)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Username", user.Username);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@Role", user.Role.ToString());
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("spUser_Create", parameters, commandType: CommandType.StoredProcedure);
            var newId = parameters.Get<int>("@NewId");
            logger.LogInformation("User created with Id {NewId} at {Timestamp:O}", newId, now);
            return newId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {Username} at {Timestamp:O}", user.Username, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<User>(
                "spUser_GetByUsername", new { Username = username }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Checked existence for username {Username} at {Timestamp:O}", username, now);
            return user is not null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking existence for username {Username} at {Timestamp:O}", username, now);
            throw;
        }
    }
}
