namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Defines contract for post like data access.
/// </summary>
public interface IPostLikeRepository
{
    /// <summary>
    /// Checks whether the specified user has already liked the given post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns><c>true</c> if the user has liked the post; otherwise, <c>false</c>.</returns>
    Task<bool> HasUserLikedAsync(int postId, int userId);

    /// <summary>
    /// Adds a like to the specified post from the specified user.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to like.</param>
    /// <param name="userId">The unique identifier of the user liking the post.</param>
    /// <returns>The unique identifier of the newly created like.</returns>
    Task<int> AddLikeAsync(int postId, int userId);

    /// <summary>
    /// Removes a like from the specified post for the specified user.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns><c>true</c> if the like was removed; otherwise, <c>false</c>.</returns>
    Task<int> RemoveLikeAsync(int postId, int userId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    /// <returns>An enumerable of <see cref="PostLike"/> matching the criteria.</returns>
    Task<IEnumerable<PostLike>> GetLikesByPostIdAsync(int postId);
}
