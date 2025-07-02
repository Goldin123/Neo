using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Application.UseCases.AddComment;
using Neo.Application.UseCases.CreatePost;
using Neo.Application.UseCases.FlagPost;
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Application.UseCases.LikePost;
using Neo.Application.UseCases.UnlikePost;
using Neo.Domain.Entities;
using System.Security.Claims;
using Neo.Application.UseCases.TagPost;

namespace Neo.Api.Controllers;

/// <summary>
/// Handles forum post-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets paged posts with filters, this does not require an auth token.
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
    /// Creates a new post by the current user, you need to be authotised first to perfom this.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0)
            return Unauthorized();

        var command = new CreatePostCommand(userId, dto.Title, dto.Content);
        var postId = await mediator.Send(command);

        return CreatedAtAction(
           nameof(Create),
           new { id = postId },
           new { postId, message = "Successfully created a post" }
       );

    }

    /// <summary>
    /// Likes a post by the current user, this only allows one like per user, users cannot like theirs own posts.
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> Like(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var likeId = await mediator.Send(new LikePostCommand(id, userId));
        if (likeId == -1)
            return BadRequest(new { message = "You cannot like a post more than once." });

        if (likeId == -2)
            return BadRequest("You cannot like your own post.");
        return Ok(new { message = "Successfully liked post.", likeId });
    }

    /// <summary>
    /// Removes a like from a post by the current user, if the .
    /// </summary>
    [HttpPost("{id}/unlike")]
    [Authorize]
    public async Task<IActionResult> Unlike(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new UnlikePostCommand(id, userId));
        
        if (result == -1)
            return BadRequest(new { message = "You have not liked this post or already unliked it." });
         
        return Ok(new { message = "Successfully removed like on the post." });

    }

    /// <summary>
    /// Flags a post as misleading or false information (moderators only).
    /// Adds a regulatory tag to the post.
    /// </summary>
    [HttpPost("{PostId}/flag")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Flag(int PostId, [FromBody] FlagPostDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId == 0) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Reason))
            return BadRequest(new { message = "Flag reason is required." });

        // --- Map reason to tag name ---
        var tagName = MapReasonToTag(dto.Reason);
        if (string.IsNullOrWhiteSpace(tagName))
            return BadRequest(new { message = "Invalid flag reason. No corresponding tag found." });

        var cmd = new FlagPostCommand(PostId, tagName, userId);

        var flagResult = await mediator.Send(cmd);
        if (!flagResult)
            return BadRequest(new { message = "Failed to flag post." });     

        // Only add a tag if the mapping found one
        if (!string.IsNullOrWhiteSpace(tagName))
            await mediator.Send(new TagPostCommand(PostId, tagName));

        return Ok(new { message = $"Sucessfully flagged the post and created a {tagName} tag on it." });
    }

    // Maps moderator reason to regulatory tag
    private static string? MapReasonToTag(string reason)
    {
        reason = reason.ToLowerInvariant();

        if (reason.Contains("mislead"))
            return "misleading";
        if (reason.Contains("false"))
            return "false_information";
        // Add more mappings as needed
        return null;
    }

    public record CreatePostDto(string Title, string Content);
    public record FlagPostDto(string Reason);
}
