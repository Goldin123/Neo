namespace Neo.Application.UseCases.LikePost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles liking a post.
/// </summary>
public sealed class LikePostHandler(
    IPostLikeRepository likeRepo,
    ILogger<LikePostHandler> logger
) : IRequestHandler<LikePostCommand, int>
{
    public async Task<int> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("User {UserId} is liking post {PostId} at {Timestamp:O}", request.UserId, request.PostId, DateTime.UtcNow);
        var likeId = await likeRepo.AddLikeAsync(request.PostId, request.UserId);
        logger.LogInformation("Like (Id: {LikeId}) added to post {PostId} by user {UserId} at {Timestamp:O}", likeId, request.PostId, request.UserId, DateTime.UtcNow);
        return likeId;
    }
}
