using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Neo.Api.Controllers;
using Neo.Application.UseCases.CreatePost;
using Xunit;

namespace Neo.Tests.Functional;

public class PostsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly PostsController _controller;

    public PostsControllerTests()
    {
        _controller = new PostsController(_mediator.Object);
    }

    [Fact]
    public async Task Create_Returns_Unauthorized_If_UserId_Missing()
    {
        // Arrange: No claims in HttpContext
        var dto = new PostsController.CreatePostDto("Test Title", "Test Content");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_Returns_CreatedAtAction_When_Valid()
    {
        // Arrange: Set up user claims and mediator
        var userId = 42;
        var dto = new PostsController.CreatePostDto("New Post", "Some content");

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };

        _mediator.Setup(m => m.Send(It.Is<CreatePostCommand>(c =>
            c.UserId == userId &&
            c.Title == dto.Title &&
            c.Content == dto.Content
        ), default)).ReturnsAsync(123); // Example postId

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        var value = created.Value!;
        var postIdProperty = value.GetType().GetProperty("postId");
        Assert.NotNull(postIdProperty);
        Assert.Equal(123, postIdProperty.GetValue(value));
    }
}
