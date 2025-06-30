namespace Neo.Infrastructure.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Data;
using System.Data;

/// <summary>
/// Provides data access functionality for <see cref="Comment"/> entities.
/// </summary>
public sealed class CommentRepository(DbContext dbContext, ILogger<CommentRepository> logger) : ICommentRepository
{
    /// <inheritdoc />
    public async Task<Comment?> GetByIdAsync(int id)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Comment>(
                "spComment_GetById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching comment by Id {Id} at {Timestamp:O}", id, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QueryAsync<Comment>(
                "spComment_GetByPostId", new { PostId = postId }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching comments for post {PostId} at {Timestamp:O}", postId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Comment comment)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PostId", comment.PostId);
            parameters.Add("@UserId", comment.UserId);
            parameters.Add("@Content", comment.Content);
            parameters.Add("@CreatedAt", comment.CreatedAt == default ? now : comment.CreatedAt);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("spComment_Create", parameters, commandType: CommandType.StoredProcedure);
            var newId = parameters.Get<int>("@NewId");
            logger.LogInformation("Comment created with Id {CommentId} for post {PostId} by user {UserId} at {Timestamp:O}", newId, comment.PostId, comment.UserId, now);
            return newId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating comment for post {PostId} by user {UserId} at {Timestamp:O}", comment.PostId, comment.UserId, now);
            throw;
        }
    }
}
