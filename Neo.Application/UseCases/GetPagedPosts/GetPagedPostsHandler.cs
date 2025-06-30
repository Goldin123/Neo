namespace Neo.Application.UseCases.GetPagedPosts;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Application.Common;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles paged post queries.
/// </summary>
public sealed class GetPagedPostsHandler(
    IPostRepository postRepo,
    ILogger<GetPagedPostsHandler> logger
) : IRequestHandler<GetPagedPostsQuery, PagedResult<Post>>
{
    public async Task<PagedResult<Post>> Handle(GetPagedPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching paged posts (Page: {Page}, Size: {PageSize}) at {Timestamp:O}", request.Page, request.PageSize, DateTime.UtcNow);
        var posts = await postRepo.GetPagedAsync(request.Page, request.PageSize, request.AuthorId, request.Start, request.End, request.Tag, request.SortBy, request.Desc);
        var result = new PagedResult<Post>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = posts.Count(), // Adjust if you have a total-count query
            Items = posts
        };
        logger.LogInformation("Fetched {Count} posts at {Timestamp:O}", result.Items.Count(), DateTime.UtcNow);
        return result;
    }
}
