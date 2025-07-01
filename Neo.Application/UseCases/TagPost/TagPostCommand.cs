namespace Neo.Application.UseCases.TagPost;

using MediatR;

/// <summary>
/// Command to tag a post with a specified tag name.
/// </summary>
public sealed record TagPostCommand(
    int PostId,
    string TagName
) : IRequest<bool>;
