namespace Neo.Application.UseCases.UnlikePost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles the UnlikePost use case.
/// </summary>
public sealed class UnlikePostHandler(
    IPostLikeRepository likeRepo,
    ILogger<UnlikePostHandler> logger
) : IRequestHandler<UnlikePostCommand, int>
{
    public async Task<int> Handle(UnlikePostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User {UserId} is removing like from post {PostId} at {Timestamp:O}", request.UserId, request.PostId, DateTime.UtcNow);
        var success = await likeRepo.RemoveLikeAsync(request.PostId, request.UserId);
        logger.LogInformation("Unlike operation for post {PostId} by user {UserId} completed with result: {Result} at {Timestamp:O}", request.PostId, request.UserId, success, DateTime.UtcNow);
        return success;
    }
}
