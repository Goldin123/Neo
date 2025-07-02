namespace Neo.Application.UseCases.LikePost;

using MediatR;

/// <summary>
/// Command to like a post.
/// </summary>
public record LikePostCommand(
    int PostId,
    int UserId
) : IRequest<int>;
