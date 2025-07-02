namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Defines contract for post data access.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Gets a post by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <returns>The <see cref="Post"/> if found; otherwise, <c>null</c>.</returns>
    Task<Post?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a paged list of posts with optional filters and sorting.
    /// </summary>
    /// <param name="page">The page number (starting from 1).</param>
    /// <param name="pageSize">The number of posts per page.</param>
    /// <param name="authorId">Optional: Filter by author user ID.</param>
    /// <param name="start">Optional: Filter by start date (inclusive).</param>
    /// <param name="end">Optional: Filter by end date (inclusive).</param>
    /// <param name="tag">Optional: Filter by tag name.</param>
    /// <param name="sortBy">Optional: Sort by property (e.g., "CreatedAt").</param>
    /// <param name="desc">Sort in descending order if <c>true</c>; ascending if <c>false</c>.</param>
    /// <returns>An enumerable of <see cref="Post"/> matching the criteria.</returns>
    Task<IEnumerable<Post>> GetPagedAsync(
        int page,
        int pageSize,
        int? authorId = null,
        DateTime? start = null,
        DateTime? end = null,
        string? tag = null,
        string? sortBy = null,
        bool desc = false);

    /// <summary>
    /// Creates a new post with optional tags.
    /// </summary>
    /// <param name="post">The post to create.</param>
    /// <returns>The unique identifier of the newly created post.</returns>
    Task<int> CreateAsync(Post post);

    /// <summary>
    /// Adds a tag to a post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="tagName">The name of the tag to add.</param>
    /// <returns><c>true</c> if the tag was added successfully; otherwise, <c>false</c>.</returns>
    Task<bool> AddTagAsync(int postId, string tagName);

    /// <summary>
    /// Flags a post as misleading or false information.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to flag.</param>
    /// <param name="reason">The reason for flagging the post.</param>
    /// <param name="moderatorId">The unique identifier of the moderator flagging the post.</param>
    /// <returns><c>true</c> if the post was flagged successfully; otherwise, <c>false</c>.</returns>
    Task<bool> FlagPostAsync(int postId, string reason, int moderatorId);

    /// <summary>
    /// Gets the total like count for a post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>The number of likes the post has received.</returns>
    Task<int> GetLikeCountAsync(int postId);
}
