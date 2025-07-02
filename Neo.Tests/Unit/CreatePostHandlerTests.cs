// Unit Test: CreatePostHandlerTests.cs
using Microsoft.Extensions.Logging;
using Moq;
using Neo.Application.UseCases.CreatePost;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Xunit;

namespace Neo.Tests.Unit;

public class CreatePostHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Post_And_Return_Id()
    {
        // Arrange
        var postRepo = new Mock<IPostRepository>();
        var logger = new Mock<ILogger<CreatePostHandler>>();
        var handler = new CreatePostHandler(postRepo.Object, logger.Object);

        var command = new CreatePostCommand(UserId: 42, Title: "Title", Content: "Content");
        postRepo.Setup(r => r.CreateAsync(It.IsAny<Post>())).ReturnsAsync(101);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.Equal(101, result);
        postRepo.Verify(r => r.CreateAsync(It.Is<Post>(p =>
            p.UserId == 42 &&
            p.Title == "Title" &&
            p.Content == "Content"
        )), Times.Once);
    }
}