namespace Neo.Application.UseCases.AddComment;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles adding a comment to a post.
/// </summary>
public sealed class AddCommentHandler(
    ICommentRepository commentRepo,
    ILogger<AddCommentHandler> logger
) : IRequestHandler<AddCommentCommand, int>
{
    public async Task<int> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding comment for post {PostId} by user {UserId} at {Timestamp:O}", request.PostId, request.UserId, DateTime.UtcNow);
        var comment = new Comment
        {
            PostId = request.PostId,
            UserId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };
        var commentId = await commentRepo.CreateAsync(comment);
        logger.LogInformation("Comment {CommentId} added to post {PostId} by user {UserId} at {Timestamp:O}", commentId, request.PostId, request.UserId, DateTime.UtcNow);
        return commentId;
    }
}
