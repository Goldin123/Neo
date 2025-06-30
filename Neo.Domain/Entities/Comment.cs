namespace Neo.Domain.Entities;

/// <summary>
/// Represents a comment made on a post within the Neo forum.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the post to which this comment belongs.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who made the comment.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the content/body of the comment.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the comment was created (in UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
