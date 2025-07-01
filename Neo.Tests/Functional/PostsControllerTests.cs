using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Neo.Api.Controllers;
using Neo.Application.Common;
using Neo.Application.UseCases.CreatePost;
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Application.UseCases.LikePost;
using Neo.Domain.Entities;
using System.Security.Claims;
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

    [Fact]
    public async Task GetPaged_ReturnsOk_WithPagedResult()
    {
        // Arrange
        var posts = new List<Post>
            {
                new Post { Id = 1, UserId = 1, Title = "First", Content = "Hello", CreatedAt = DateTime.UtcNow }
            };

        var pagedResult = new PagedResult<Post>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = posts
        };

        _mediator.Setup(m => m.Send(
            It.Is<GetPagedPostsQuery>(q => q.Page == 1 && q.PageSize == 10),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<PagedResult<Post>>(okResult.Value);

        Assert.Single(actual.Items);
        Assert.Equal("First", actual.Items.First().Title);

    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var pagedResult = new PagedResult<Post>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            Items = new List<Post>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetPagedPostsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<PagedResult<Post>>(okResult.Value);

        Assert.Empty(actual.Items);
    }

    [Fact]
    public async Task Like_Returns_Ok_With_LikeId_When_Successful()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<LikePostCommand>(), default)).ReturnsAsync(42);

        var controller = new PostsController(mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "10") }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.Like(25);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;
        var likeIdProp = value.GetType().GetProperty("likeId");
        Assert.NotNull(likeIdProp);
        var likeId = likeIdProp.GetValue(value);
        Assert.Equal(42, likeId);
    }


    [Fact]
    public async Task Like_Returns_Unauthorized_If_User_Not_Authenticated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await controller.Like(25);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
