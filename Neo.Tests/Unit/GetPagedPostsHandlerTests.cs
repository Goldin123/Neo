using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Neo.Application.DTOs; // Use correct namespace if different
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Neo.Tests.Unit
{
    public class GetPagedPostsHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsPagedResult_WithPosts()
        {
            // Arrange
            var posts = new List<Post>
            {
                new Post { Id = 1, UserId = 1, Title = "Post 1", Content = "A", CreatedAt = DateTime.UtcNow },
                new Post { Id = 2, UserId = 2, Title = "Post 2", Content = "B", CreatedAt = DateTime.UtcNow }
            };

            var postRepoMock = new Mock<IPostRepository>();
            var commentRepoMock = new Mock<ICommentRepository>();
            var likeRepoMock = new Mock<IPostLikeRepository>();
            var tagRepoMock = new Mock<ITagRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var loggerMock = new Mock<ILogger<GetPagedPostsHandler>>();

            postRepoMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, null, false))
                .ReturnsAsync(posts);

            // Comments: Only UserId provided
            commentRepoMock.Setup(r => r.GetByPostIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int postId) =>
                {
                    return new List<Comment>
                    {
                        new Comment { Id = 1, PostId = postId, UserId = 10, Content = $"Nice post {postId}", CreatedAt = DateTime.UtcNow }
                    };
                });

            // Likes: Only UserId provided
            likeRepoMock.Setup(r => r.GetLikesByPostIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int postId) =>
                {
                    return new List<PostLike>
                    {
                        new PostLike { Id = 1, PostId = postId, UserId = 11, CreatedAt = DateTime.UtcNow }
                    };
                });

            // Tags
            tagRepoMock.Setup(r => r.GetTagsByPostIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Tag>
                {
                    new Tag { Id = 1, Name = "TestTag" }
                });

            // Users: Maps UserId -> UserName ("User" + Id)
            userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int userId) =>
                {
                    return new User { Id = userId, Username = $"User{userId}" };
                });

            var handler = new GetPagedPostsHandler(
                postRepoMock.Object,
                commentRepoMock.Object,
                likeRepoMock.Object,
                tagRepoMock.Object,
                userRepoMock.Object,
                loggerMock.Object
            );

            var query = new GetPagedPostsQuery(1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(posts.Count, result.TotalCount);
            Assert.Equal(posts.Count, result.Items.Count);

            var postDto = result.Items.First();
            Assert.Equal(posts[0].Id, postDto.PostId);
            Assert.Equal(posts[0].Title, postDto.PostTitle);
            Assert.Equal(posts[0].Content, postDto.PostContent);

            // CreatedUser mapping via UserRepo
            Assert.NotNull(postDto.CreatedUser);
            Assert.Equal($"User{posts[0].UserId}", postDto.CreatedUser.UserName);

            // Tags
            Assert.Single(postDto.Tags);
            Assert.Equal("TestTag", postDto.Tags[0].TagName);

            // Comments: UserId mapped to UserName
            Assert.Single(postDto.Comments);
            Assert.Equal("User10", postDto.Comments[0].CommentUserName);

            // Likes: UserId mapped to UserName
            Assert.Single(postDto.Likes);
            Assert.Equal("User11", postDto.Likes[0].LikedUserName);

            // Summary
            Assert.Equal(1, postDto.Summary.TotalTags);
            Assert.Equal(1, postDto.Summary.TotalComments);
            Assert.Equal(1, postDto.Summary.TotalLikes);
        }

        [Fact]
        public async Task Handle_ReturnsEmpty_WhenNoPostsFound()
        {
            // Arrange
            var postRepoMock = new Mock<IPostRepository>();
            var commentRepoMock = new Mock<ICommentRepository>();
            var likeRepoMock = new Mock<IPostLikeRepository>();
            var tagRepoMock = new Mock<ITagRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var loggerMock = new Mock<ILogger<GetPagedPostsHandler>>();

            postRepoMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, null, false))
                .ReturnsAsync(new List<Post>());

            var handler = new GetPagedPostsHandler(
                postRepoMock.Object,
                commentRepoMock.Object,
                likeRepoMock.Object,
                tagRepoMock.Object,
                userRepoMock.Object,
                loggerMock.Object
            );

            var query = new GetPagedPostsQuery(1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }
}
