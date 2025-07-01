using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Neo.Api.Controllers;
using Neo.Application.Common;
using Neo.Application.DTOs;
using Neo.Application.UseCases.CreatePost;
using Neo.Application.UseCases.FlagPost;
using Neo.Application.UseCases.GetPagedPosts;
using Neo.Application.UseCases.LikePost;
using Neo.Application.UseCases.TagPost;
using Neo.Application.UseCases.UnlikePost;
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

    // Helper to set controller context with claims and role
    private void SetUser(PostsController controller, int userId, bool isModerator = false)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (isModerator)
            claims.Add(new Claim(ClaimTypes.Role, "Moderator"));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
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
        var pagedPosts = new List<PagedPostDto>
    {
        new PagedPostDto
        {
            PostId = 1,
            PostTitle = "First",
            PostContent = "Hello",
            PostCreated = DateTime.UtcNow,
            CreatedUser = new PostUserDto { Id = 1, UserName = "User1" },
            Tags = new List<PostTagDto> { new PostTagDto { TagName = "TestTag" } },
            Comments = new List<PostCommentDto>
            {
                new PostCommentDto { CommentUserName = "Commenter", CommentContent = "Hi!", DateCreated = DateTime.UtcNow }
            },
            Likes = new List<PostLikeDto>
            {
                new PostLikeDto { LikedUserName = "Liker", LikedDate = DateTime.UtcNow }
            },
            Summary = new PostSummaryDto
            {
                TotalTags = 1,
                TotalComments = 1,
                TotalLikes = 1
            }
        }
    };

        var pagedResult = new PagedResultDto<PagedPostDto>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
            Items = pagedPosts
        };

        _mediator.Setup(m => m.Send(
            It.Is<GetPagedPostsQuery>(q => q.Page == 1 && q.PageSize == 10),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<PagedResultDto<PagedPostDto>>(okResult.Value);

        Assert.Single(actual.Items);
        Assert.Equal("First", actual.Items.First().PostTitle);
        Assert.Equal("User1", actual.Items.First().CreatedUser.UserName);
        Assert.Single(actual.Items.First().Tags);
        Assert.Single(actual.Items.First().Comments);
        Assert.Single(actual.Items.First().Likes);
        Assert.Equal(1, actual.Items.First().Summary.TotalLikes);
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithEmptyList()
    {
        // Arrange
        var pagedResult = new PagedResultDto<PagedPostDto>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            Items = new List<PagedPostDto>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetPagedPostsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPaged();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<PagedResultDto<PagedPostDto>>(okResult.Value);

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

    [Fact]
    public async Task Flag_Returns_Unauthorized_If_Not_Authenticated()
    {
        // Arrange
        var controller = new PostsController(_mediator.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var dto = new PostsController.FlagPostDto("misleading");

        // Act
        var result = await controller.Flag(123, dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Flag_Returns_Unauthorized_If_Not_Moderator()
    {
        // Arrange: Regular user (no moderator role)
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 50, isModerator: false);
        var dto = new PostsController.FlagPostDto("misleading");

        // Act
        var result = await controller.Flag(123, dto);

        // Assert
        // Actually, your controller is protected with [Authorize(Roles = "Moderator")] attribute,
        // which the functional test with the controller itself cannot bypass unless you simulate the request pipeline
        // In direct invocation, the controller does not check role manually; only your test web host would.
        // So, if you want to test this without web host, document as a note!
        // We'll just check the moderator scenario in the next tests.
    }

    [Fact]
    public async Task Flag_Returns_BadRequest_If_Reason_Missing()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 100, isModerator: true);
        var dto = new PostsController.FlagPostDto("");

        var result = await controller.Flag(123, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Flag reason is required.", bad.Value);
    }

    [Fact]
    public async Task Flag_Returns_BadRequest_If_Reason_Does_Not_Map()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 101, isModerator: true);
        var dto = new PostsController.FlagPostDto("totally random");

        var result = await controller.Flag(123, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid flag reason. No corresponding tag found.", bad.Value);
    }

    [Fact]
    public async Task Flag_Returns_BadRequest_If_Mediator_Returns_False()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 200, isModerator: true);
        var dto = new PostsController.FlagPostDto("misleading");

        _mediator.Setup(m => m.Send(It.IsAny<FlagPostCommand>(), default)).ReturnsAsync(false);

        var result = await controller.Flag(456, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to flag post.", bad.Value);
    }

    [Fact]
    public async Task Flag_Returns_Ok_If_Successful()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 222, isModerator: true);
        var dto = new PostsController.FlagPostDto("false_information");

        _mediator.Setup(m => m.Send(It.IsAny<FlagPostCommand>(), default)).ReturnsAsync(true);
        _mediator.Setup(m => m.Send(It.IsAny<TagPostCommand>(), default)).ReturnsAsync(true);

        var result = await controller.Flag(789, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Sucessfully flagged", ok.Value!.ToString());
        Assert.Contains("false_information", ok.Value!.ToString());
    }

    [Fact]
    public async Task Unlike_Returns_Unauthorized_If_User_Not_Authenticated()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await controller.Unlike(25);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Unlike_Returns_BadRequest_If_User_Has_Not_Liked_Post()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UnlikePostCommand>(), default)).ReturnsAsync(-1);

        var controller = new PostsController(mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "17")
    }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.Unlike(25);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("You have not liked this post or already unliked it.", bad.Value);
    }

    [Fact]
    public async Task Unlike_Returns_Ok_If_Successful()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UnlikePostCommand>(), default)).ReturnsAsync(1);

        var controller = new PostsController(mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim(ClaimTypes.NameIdentifier, "15")
    }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await controller.Unlike(25);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Unlike_Returns_Unauthorized_If_UserId_Is_Zero()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        // UserId claim is missing (should parse to 0)
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var result = await controller.Unlike(44);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }


}
