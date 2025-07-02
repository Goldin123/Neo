namespace Neo.Application.UseCases.AddComment;

using MediatR;

/// <summary>
/// Command to add a comment to a post.
/// </summary>
public record AddCommentCommand(
    int PostId,
    int UserId,
    string Content
) : IRequest<int>;
