using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Neo.Application.Common;
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

            var repoMock = new Mock<IPostRepository>();
            repoMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, null, false))
                .ReturnsAsync(posts);

            var loggerMock = new Mock<ILogger<GetPagedPostsHandler>>();

            var handler = new GetPagedPostsHandler(repoMock.Object, loggerMock.Object);
            var query = new GetPagedPostsQuery(1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(posts.Count, result.TotalCount); // Since you use posts.Count() for TotalCount
            Assert.Equal(posts, result.Items);
        }

        [Fact]
        public async Task Handle_ReturnsEmpty_WhenNoPostsFound()
        {
            // Arrange
            var repoMock = new Mock<IPostRepository>();
            repoMock.Setup(r => r.GetPagedAsync(1, 10, null, null, null, null, null, false))
                .ReturnsAsync(new List<Post>());

            var loggerMock = new Mock<ILogger<GetPagedPostsHandler>>();
            var handler = new GetPagedPostsHandler(repoMock.Object, loggerMock.Object);
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
