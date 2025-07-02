namespace Neo.Application.UseCases.CreatePost;

using MediatR;

/// <summary>
/// Command to create a new post.
/// </summary>
public record CreatePostCommand(
    int UserId,
    string Title,
    string Content
) : IRequest<int>;
