namespace Neo.Application.UseCases.FlagPost;

using MediatR;

/// <summary>
/// Command to flag a post.
/// </summary>
public record FlagPostCommand(
    int PostId,
    string Reason,
    int ModeratorId
) : IRequest<bool>;
