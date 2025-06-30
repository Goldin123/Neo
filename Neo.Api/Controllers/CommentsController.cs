using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Application.UseCases.AddComment;
using Neo.Application.UseCases.GetCommentsByPost;
using System.Security.Claims;

namespace Neo.Api.Controllers;

/// <summary>
/// Handles comment-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets comments for a post.
    /// </summary>
    [HttpGet("by-post/{postId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPost(int postId)
    {
        var comments = await mediator.Send(new GetCommentsByPostQuery(postId));
        return Ok(comments);
    }

    /// <summary>
    /// Adds a comment to a post.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] AddCommentCommand command)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var newCommand = command with { UserId = userId };
        var commentId = await mediator.Send(newCommand);
        return Ok(new { commentId });
    }
}
