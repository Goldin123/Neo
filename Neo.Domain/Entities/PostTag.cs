namespace Neo.Domain.Entities;

/// <summary>
/// Represents the association between a post and a tag in the Neo forum.
/// </summary>
public class PostTag
{
    /// <summary>
    /// Gets or sets the unique identifier of the post.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the tag.
    /// </summary>
    public int TagId { get; set; }
}
