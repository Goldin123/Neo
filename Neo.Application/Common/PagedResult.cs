namespace Neo.Application.Common;

/// <summary>
/// Represents a paged result for a query.
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Gets or sets the total number of records.
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Gets or sets the collection of data items.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}
