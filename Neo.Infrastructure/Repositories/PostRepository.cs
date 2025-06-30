namespace Neo.Infrastructure.Repositories;

using Dapper;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Data;
using System.Data;

/// <summary>
/// Provides data access functionality for <see cref="Post"/> entities.
/// </summary>
public sealed class PostRepository(DbContext dbContext, ILogger<PostRepository> logger) : IPostRepository
{
    /// <inheritdoc />
    public async Task<Post?> GetByIdAsync(int id)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Post>(
                "spPost_GetById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching post by Id {Id} at {Timestamp:O}", id, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Post>> GetPagedAsync(
        int page, int pageSize, int? authorId = null, DateTime? start = null, DateTime? end = null, string? tag = null, string? sortBy = null, bool desc = false)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            return await conn.QueryAsync<Post>(
                "spPost_GetPaged",
                new
                {
                    Page = page,
                    PageSize = pageSize,
                    AuthorId = authorId,
                    Start = start,
                    End = end,
                    Tag = tag,
                    SortBy = sortBy,
                    Descending = desc ? 1 : 0
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving paged posts at {Timestamp:O}", now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Post post, IEnumerable<string>? tags = null)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", post.UserId);
            parameters.Add("@Title", post.Title);
            parameters.Add("@Content", post.Content);
            parameters.Add("@CreatedAt", post.CreatedAt == default ? now : post.CreatedAt);
            parameters.Add("@NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conn.ExecuteAsync("spPost_Create", parameters, commandType: CommandType.StoredProcedure);
            var postId = parameters.Get<int>("@NewId");

            if (tags is not null)
            {
                foreach (var tag in tags.Distinct())
                {
                    await conn.ExecuteAsync("spPost_AddTag",
                        new { PostId = postId, TagName = tag },
                        commandType: CommandType.StoredProcedure);
                }
            }
            logger.LogInformation("Post created with Id {PostId} by user {UserId} at {Timestamp:O}", postId, post.UserId, now);
            return postId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating post by user {UserId} at {Timestamp:O}", post.UserId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> AddTagAsync(int postId, string tagName)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            await conn.ExecuteAsync("spPost_AddTag", new { PostId = postId, TagName = tagName }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Tag '{TagName}' added to post {PostId} at {Timestamp:O}", tagName, postId, now);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding tag '{TagName}' to post {PostId} at {Timestamp:O}", tagName, postId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> FlagPostAsync(int postId, string reason, int moderatorId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            await conn.ExecuteAsync("spPost_Flag", new { PostId = postId, ModeratorId = moderatorId, Reason = reason }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Post {PostId} flagged by moderator {ModeratorId} at {Timestamp:O}", postId, moderatorId, now);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error flagging post {PostId} by moderator {ModeratorId} at {Timestamp:O}", postId, moderatorId, now);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetLikeCountAsync(int postId)
    {
        var now = DateTime.UtcNow;
        try
        {
            using var conn = dbContext.CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>("spPost_GetLikeCount", new { PostId = postId }, commandType: CommandType.StoredProcedure);
            logger.LogInformation("Like count {Count} for post {PostId} at {Timestamp:O}", count, postId, now);
            return count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting like count for post {PostId} at {Timestamp:O}", postId, now);
            throw;
        }
    }
}
