namespace Neo.Infrastructure.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Data;
using System.Data;

/// <summary>
/// Provides data access functionality for <see cref="Tag"/> entities.
/// </summary>
public sealed class TagRepository(DbContext dbContext, ILogger<TagRepository> logger) : ITagRepository
{
    /// <inheritdoc />
    public async Task<Tag?> GetByIdAsync(int id)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Tag>(
                "spTag_GetById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tag by Id {Id} at {Timestamp:O}", id, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Tag?> GetByNameAsync(string name)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Tag>(
                "spTag_GetByName", new { Name = name }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tag by name {Name} at {Timestamp:O}", name, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(string name)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Name", name);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("spTag_Create", parameters, commandType: CommandType.StoredProcedure);
            var tagId = parameters.Get<int>("@NewId");
            logger.LogInformation("Tag created with Id {TagId} and name {Name} at {Timestamp:O}", tagId, name, now);
            return tagId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tag with name {Name} at {Timestamp:O}", name, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QueryAsync<Tag>(
                "spTag_GetAll", commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all tags at {Timestamp:O}", now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QueryAsync<Tag>(
                "spTag_GetTagsByPostId", new { PostId = postId }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tags for post {PostId} at {Timestamp:O}", postId, now);
            throw;
        }
    }
}
