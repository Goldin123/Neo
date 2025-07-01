using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Application.UseCases.AddComment;
using Neo.Application.UseCases.FlagPost;
using Neo.Application.UseCases.GetCommentsByPost;
using Neo.Domain.Entities;
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
    /// Adds a comment to a post by the current user.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] AddCommentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();


        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Comment content is required.");

        var newCommand = new AddCommentCommand(dto.PostId, userId, dto.Content);
        var commentId = await mediator.Send(newCommand);
        return Ok(new { commentId });
    }

    public record AddCommentDto(int PostId, string Content);
}
