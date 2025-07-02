namespace Neo.Domain.Entities;

/// <summary>
/// Represents a tag used to categorize or label posts in the Neo forum.
/// </summary>
public class Tag
{
    /// <summary>
    /// Gets or sets the unique identifier for the tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the tag.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
