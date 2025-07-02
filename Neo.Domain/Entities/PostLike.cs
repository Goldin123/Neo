namespace Neo.Domain.Entities;

/// <summary>
/// Represents a 'like' given by a user to a post in the Neo forum.
/// </summary>
public class PostLike
{
    /// <summary>
    /// Gets or sets the unique identifier for the like.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the post that was liked.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who liked the post.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the like was added (in UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
