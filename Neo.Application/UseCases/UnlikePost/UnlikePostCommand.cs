namespace Neo.Application.UseCases.UnlikePost;

using MediatR;

/// <summary>
/// Command to remove a like from a post.
/// </summary>
public record UnlikePostCommand(int PostId, int UserId) : IRequest<bool>;
