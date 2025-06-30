namespace Neo.Application.UseCases.GetPagedPosts;

using MediatR;
using Neo.Application.Common;
using Neo.Domain.Entities;

/// <summary>
/// Query to get paged posts.
/// </summary>
public record GetPagedPostsQuery(
    int Page,
    int PageSize,
    int? AuthorId = null,
    DateTime? Start = null,
    DateTime? End = null,
    string? Tag = null,
    string? SortBy = null,
    bool Desc = false
) : IRequest<PagedResult<Post>>;
