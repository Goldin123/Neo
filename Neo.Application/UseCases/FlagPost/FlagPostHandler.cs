namespace Neo.Application.UseCases.FlagPost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles flagging a post.
/// </summary>
public sealed class FlagPostHandler(
    IPostRepository postRepo,
    ILogger<FlagPostHandler> logger
) : IRequestHandler<FlagPostCommand, bool>
{
    public async Task<bool> Handle(FlagPostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Moderator {ModeratorId} flagging post {PostId} for reason: {Reason} at {Timestamp:O}", request.ModeratorId, request.PostId, request.Reason, DateTime.UtcNow);
        var result = await postRepo.FlagPostAsync(request.PostId, request.Reason, request.ModeratorId);
        logger.LogInformation("Post {PostId} flagged: {Result} at {Timestamp:O}", request.PostId, result, DateTime.UtcNow);
        return result;
    }
}
