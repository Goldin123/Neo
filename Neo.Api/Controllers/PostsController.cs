using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Application.UseCases.AddComment;
using Neo.Application.UseCases.CreatePost;
using Neo.Application.UseCases.FlagPost;
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Application.UseCases.LikePost;
using Neo.Domain.Entities;
using System.Security.Claims;

namespace Neo.Api.Controllers;

/// <summary>
/// Handles forum post-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets paged posts with filters.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? authorId = null,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false)
    {
        var result = await mediator.Send(new GetPagedPostsQuery(page, pageSize, authorId, start, end, tag, sortBy, desc));
        return Ok(result);
    }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostCommand command)
    {
        // get userId from JWT claims
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var newCommand = command with { UserId = userId };
        var postId = await mediator.Send(newCommand);
        return CreatedAtAction(nameof(GetPaged), new { id = postId }, new { postId });
    }

    /// <summary>
    /// Likes a post.
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> Like(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var likeId = await mediator.Send(new LikePostCommand(id, userId));
        if (likeId == -1)
            return BadRequest("You cannot like your own post or like a post more than once.");
        return Ok(new { likeId });
    }

    /// <summary>
    /// Flags a post as misleading or false information (moderators only).
    /// </summary>
    [HttpPost("{id}/flag")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Flag(int id, [FromBody] FlagPostCommand cmd)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var flagResult = await mediator.Send(cmd with { PostId = id, ModeratorId = userId });
        if (!flagResult)
            return BadRequest("Failed to flag post.");
        return Ok();
    }
}
