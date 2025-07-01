namespace Neo.Infrastructure.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Data;
using System.Data;

/// <summary>
/// Provides data access functionality for <see cref="PostLike"/> entities.
/// </summary>
public sealed class PostLikeRepository(DbContext dbContext, ILogger<PostLikeRepository> logger) : IPostLikeRepository
{
    /// <inheritdoc />
    public async Task<bool> HasUserLikedAsync(int postId, int userId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "spPostLike_HasUserLiked", new { PostId = postId, UserId = userId }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Checked if user {UserId} liked post {PostId}: {Liked} at {Timestamp:O}", userId, postId, count > 0, now);
            return count > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if user {UserId} liked post {PostId} at {Timestamp:O}", userId, postId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> AddLikeAsync(int postId, int userId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PostId", postId);
            parameters.Add("@UserId", userId);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("spPostLike_Add", parameters, commandType: CommandType.StoredProcedure);
            var newId = parameters.Get<int>("@NewId");
            logger.LogInformation("Like added for post {PostId} by user {UserId} at {Timestamp:O}", postId, userId, now);
            return newId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding like for post {PostId} by user {UserId} at {Timestamp:O}", postId, userId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> RemoveLikeAsync(int postId, int userId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var likeId = await conn.ExecuteAsync("spPostLike_Remove", new { PostId = postId, UserId = userId }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Like removed for post {PostId} by user {UserId} at {Timestamp:O}", postId, userId, now);
            return likeId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing like for post {PostId} by user {UserId} at {Timestamp:O}", postId, userId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PostLike>> GetLikesByPostIdAsync(int postId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var likes = await conn.QueryAsync<PostLike>(
                "spPostLike_GetByPostId", // Make sure this SP exists and returns UserId, UserName, CreatedAt
                new { PostId = postId },
                commandType: CommandType.StoredProcedure
            );
            logger.LogInformation("Fetched {Count} likes for post {PostId} at {Timestamp:O}", likes.Count(), postId, now);
            return likes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching likes for post {PostId} at {Timestamp:O}", postId, now);
            throw;
        }
    }

}
