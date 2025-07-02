namespace Neo.Domain.Interfaces;

using Neo.Domain.Entities;

/// <summary>
/// Defines contract for tag data access.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Gets a tag by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag.</param>
    /// <returns>The <see cref="Tag"/> if found; otherwise, <c>null</c>.</returns>
    Task<Tag?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a tag by its name.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <returns>The <see cref="Tag"/> if found; otherwise, <c>null</c>.</returns>
    Task<Tag?> GetByNameAsync(string name);

    /// <summary>
    /// Creates a new tag and returns its unique identifier.
    /// </summary>
    /// <param name="name">The name of the tag to create.</param>
    /// <returns>The unique identifier of the newly created tag.</returns>
    Task<int> CreateAsync(string name);

    /// <summary>
    /// Gets all tags in the system.
    /// </summary>
    /// <returns>An enumerable of all <see cref="Tag"/> entities.</returns>
    Task<IEnumerable<Tag>> GetAllAsync();

    /// <summary>
    /// Gets all tags associated with a specific post.
    /// </summary>
    /// <param name="postId">The unique identifier of the post.</param>
    /// <returns>An enumerable of <see cref="Tag"/> associated with the post.</returns>
    Task<IEnumerable<Tag>> GetTagsByPostIdAsync(int postId);
}
