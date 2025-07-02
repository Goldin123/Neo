namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Defines contract for comment data access.
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Gets a comment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the comment.</param>
    /// <returns>The <see cref="Comment"/> if found; otherwise, <c>null</c>.</returns>
    Task<Comment?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all comments associated with a specific post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>An enumerable of <see cref="Comment"/>.</returns>
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="comment">The comment to create.</param>
    /// <returns>The unique identifier of the newly created comment.</returns>
    Task<int> CreateAsync(Comment comment);
}
