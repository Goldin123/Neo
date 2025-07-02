namespace Neo.Application.UseCases.TagPost;

using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Domain.Interfaces;
/// <summary>
/// Handles adding a tag to a post. If the tag does not exist, creates it first.
/// </summary>
public sealed class TagPostHandler(
    ITagRepository tagRepo,
    IPostRepository postRepo,
    ILogger<TagPostHandler> logger
) : IRequestHandler<TagPostCommand, bool>
{
    public async Task<bool> Handle(TagPostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tagging post {PostId} with tag '{TagName}' at {Timestamp:O}", request.PostId, request.TagName, DateTime.UtcNow);

        // 1. Ensure the tag exists (create if not found)
        var tag = await tagRepo.GetByNameAsync(request.TagName);
        if (tag == null)
        {
            logger.LogInformation("Tag '{TagName}' does not exist. Creating new tag.", request.TagName);
            var tagId = await tagRepo.CreateAsync(request.TagName);
            if (tagId <= 0)
            {
                logger.LogError("Failed to create tag '{TagName}' for post {PostId}.", request.TagName, request.PostId);
                return false;
            }
        }

        // 2. Add the tag to the post
        var result = await postRepo.AddTagAsync(request.PostId, request.TagName);
        if (result)
            logger.LogInformation("Tag '{TagName}' added to post {PostId}.", request.TagName, request.PostId);
        else
            logger.LogWarning("Failed to add tag '{TagName}' to post {PostId}.", request.TagName, request.PostId);

        return result;
    }
}
