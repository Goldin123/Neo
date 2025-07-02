namespace Neo.Application.UseCases.GetCommentsByPost;

using MediatR;
using Neo.Domain.Entities;

/// <summary>
/// Query to get all comments for a post.
/// </summary>
public record GetCommentsByPostQuery(
    int PostId
) : IRequest<IEnumerable<Comment>>;
