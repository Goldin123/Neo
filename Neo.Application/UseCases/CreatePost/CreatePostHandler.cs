namespace Neo.Application.UseCases.CreatePost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

/// <summary>
/// Handles creation of a new post.
/// </summary>
public sealed class CreatePostHandler(
    IPostRepository postRepo,
    ILogger<CreatePostHandler> logger
) : IRequestHandler<CreatePostCommand, int>
{
    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating post for user {UserId} at {Timestamp:O}", request.UserId, DateTime.UtcNow);
        var post = new Post
        {
            UserId = request.UserId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };
        var postId = await postRepo.CreateAsync(post);
        logger.LogInformation("Post {PostId} created successfully at {Timestamp:O}", postId, DateTime.UtcNow);
        return postId;
    }
}
