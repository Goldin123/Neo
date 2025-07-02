namespace Neo.Application.UseCases.GetCommentsByPost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles fetching comments for a post.
/// </summary>
public sealed class GetCommentsByPostHandler(
    ICommentRepository commentRepo,
    ILogger<GetCommentsByPostHandler> logger
) : IRequestHandler<GetCommentsByPostQuery, IEnumerable<Comment>>
{
    public async Task<IEnumerable<Comment>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching comments for post {PostId} at {Timestamp:O}", request.PostId, DateTime.UtcNow);
        var comments = await commentRepo.GetByPostIdAsync(request.PostId);
        logger.LogInformation("Fetched {Count} comments for post {PostId} at {Timestamp:O}", comments.Count(), request.PostId, DateTime.UtcNow);
        return comments;
    }
}
