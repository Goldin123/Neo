namespace Neo.Domain.Entities;

/// <summary>
/// Represents a forum post.
/// </summary>
public class Post
{
    /// <summary>
    /// Gets or sets the unique identifier for the post.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the post.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the title of the post.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content/body of the post.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the post was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the post is flagged by a moderator.
    /// </summary>
    public bool IsFlagged { get; set; }

    /// <summary>
    /// Gets or sets the reason for flagging the post, if any.
    /// </summary>
    public string? FlagReason { get; set; }
}
