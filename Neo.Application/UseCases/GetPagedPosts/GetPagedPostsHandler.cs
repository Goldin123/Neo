using MediatR;
using Microsoft.Extensions.Logging;
using Neo.Application.DTOs;
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Domain.Interfaces;

public sealed class GetPagedPostsHandler(
    IPostRepository postRepo,
    ICommentRepository commentRepo,
    IPostLikeRepository likeRepo,
    ITagRepository tagRepo,
    IUserRepository userRepo, // Needed for user details
    ILogger<GetPagedPostsHandler> logger
) : IRequestHandler<GetPagedPostsQuery, PagedResultDto<PagedPostDto>>
{
    public async Task<PagedResultDto<PagedPostDto>> Handle(GetPagedPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching paged posts (Page: {Page}, Size: {PageSize}) at {Timestamp:O}", request.Page, request.PageSize, DateTime.UtcNow);
        var posts = (await postRepo.GetPagedAsync(request.Page, request.PageSize, request.AuthorId, request.Start, request.End, request.Tag, request.SortBy, request.Desc)).ToList();

        var pagedPosts = new List<PagedPostDto>();

        foreach (var post in posts)
        {
            var userTask = userRepo.GetByIdAsync(post.UserId);
            var tagsTask = tagRepo.GetTagsByPostIdAsync(post.Id);
            var commentsTask = commentRepo.GetByPostIdAsync(post.Id);
            var likesTask = likeRepo.GetLikesByPostIdAsync(post.Id);

            await Task.WhenAll(userTask, tagsTask, commentsTask, likesTask);

            var user = await userTask;
            var tags = (await tagsTask).Select(t => new PostTagDto { TagName = t.Name }).ToList();

            var comments = (await commentsTask).ToList();
            var likes = (await likesTask).ToList();

            // Collect all unique UserIds for comments and likes
            var commentUserIds = comments.Select(c => c.UserId).Distinct();
            var likeUserIds = likes.Select(l => l.UserId).Distinct();
            var allUserIds = commentUserIds.Concat(likeUserIds).Distinct();

            // Fetch all needed users (avoid N+1 problem)
            var userDict = new Dictionary<int, Neo.Domain.Entities.User>();
            foreach (var uid in allUserIds)
                userDict[uid] = await userRepo.GetByIdAsync(uid);

            // Map comments using fetched user names
            var commentDtos = comments.Select(c =>
                new PostCommentDto
                {
                    CommentUserName = userDict.ContainsKey(c.UserId) ? userDict[c.UserId].Username : c.UserId.ToString(),
                    CommentContent = c.Content,
                    DateCreated = c.CreatedAt
                }
            ).ToList();

            // Map likes using fetched user names
            var likeDtos = likes.Select(l =>
                new PostLikeDto
                {
                    LikedUserName = userDict.ContainsKey(l.UserId) ? userDict[l.UserId].Username : l.UserId.ToString(),
                    LikedDate = l.CreatedAt
                }
            ).ToList();

            pagedPosts.Add(new PagedPostDto
            {
                PostId = post.Id,
                PostTitle = post.Title,
                PostContent = post.Content,
                PostCreated = post.CreatedAt,
                IsPostFlagged = post.IsFlagged,
                FlaggedReason = post.FlagReason,
                CreatedUser = new PostUserDto
                {
                    Id = user.Id,
                    UserName = user.Username // Make sure this is UserName, not Username
                },
                Tags = tags,
                Comments = commentDtos,
                Likes = likeDtos,
                Summary = new PostSummaryDto
                {
                    TotalTags = tags.Count,
                    TotalComments = commentDtos.Count,
                    TotalLikes = likeDtos.Count
                }
            });
        }


        var result = new PagedResultDto<PagedPostDto>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = posts.Count, // Or get total count from repository
            Items = pagedPosts
        };

        logger.LogInformation("Fetched {Count} posts at {Timestamp:O}", result.Items.Count, DateTime.UtcNow);
        return result;
    }
}
