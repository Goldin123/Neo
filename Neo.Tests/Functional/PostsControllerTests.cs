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
        var dto = new PostsController.CreatePostDto("Test Title", "Test Content");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.Create(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_Returns_CreatedAtAction_When_Valid()
    {
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
        ), default)).ReturnsAsync(123);

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var value = created.Value!;
        var postIdProperty = value.GetType().GetProperty("postId");
        var messageProperty = value.GetType().GetProperty("message");
        Assert.NotNull(postIdProperty);
        Assert.NotNull(messageProperty);
        Assert.Equal(123, postIdProperty.GetValue(value));
        Assert.Equal("Successfully created a post", messageProperty.GetValue(value));
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithPagedResult()
    {
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

        var result = await _controller.GetPaged();

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
        var pagedResult = new PagedResultDto<PagedPostDto>
        {
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            Items = new List<PagedPostDto>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetPagedPostsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _controller.GetPaged();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var actual = Assert.IsType<PagedResultDto<PagedPostDto>>(okResult.Value);

        Assert.Empty(actual.Items);
    }

    [Fact]
    public async Task Like_Returns_Ok_With_LikeId_When_Successful()
    {
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

        var result = await controller.Like(25);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;
        var messageProp = value.GetType().GetProperty("message");
        var likeIdProp = value.GetType().GetProperty("likeId");
        Assert.NotNull(messageProp);
        Assert.NotNull(likeIdProp);
        Assert.Equal("Successfully liked post.", messageProp.GetValue(value));
        Assert.Equal(42, likeIdProp.GetValue(value));
    }

    [Fact]
    public async Task Like_Returns_BadRequest_If_Already_Liked()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<LikePostCommand>(), default)).ReturnsAsync(-1);

        var controller = new PostsController(mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10") }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await controller.Like(25);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var value = bad.Value!;
        var msgProp = value.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("You cannot like a post more than once.", msgProp.GetValue(value));
    }

    [Fact]
    public async Task Like_Returns_BadRequest_If_User_Likes_Own_Post()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<LikePostCommand>(), default)).ReturnsAsync(-2);

        var controller = new PostsController(mediatorMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "10") }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await controller.Like(25);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("You cannot like your own post.", bad.Value);
    }

    [Fact]
    public async Task Like_Returns_Unauthorized_If_User_Not_Authenticated()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await controller.Like(25);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Flag_Returns_Unauthorized_If_Not_Authenticated()
    {
        var controller = new PostsController(_mediator.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var dto = new PostsController.FlagPostDto("misleading");

        var result = await controller.Flag(123, dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Flag_Returns_BadRequest_If_Reason_Missing()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 200, isModerator: true);
        var dto = new PostsController.FlagPostDto("misleading");

        _mediator.Setup(m => m.Send(It.IsAny<FlagPostCommand>(), default)).ReturnsAsync(false);

        var result = await controller.Flag(456, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var msgProp = bad.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Failed to flag post.", msgProp.GetValue(bad.Value));
    }

    [Fact]
    public async Task Flag_Returns_BadRequest_If_Reason_Does_Not_Map()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 101, isModerator: true);
        var dto = new PostsController.FlagPostDto("totally random");

        var result = await controller.Flag(123, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var msgProp = bad.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Invalid flag reason. No corresponding tag found.", msgProp.GetValue(bad.Value));
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
        var msgProp = bad.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Failed to flag post.", msgProp.GetValue(bad.Value));
    }

    [Fact]
    public async Task Flag_Returns_Ok_If_Successful()
    {
        var controller = new PostsController(_mediator.Object);
        SetUser(controller, userId: 222, isModerator: true);
        var dto = new PostsController.FlagPostDto("false information");

        _mediator.Setup(m => m.Send(It.IsAny<FlagPostCommand>(), default)).ReturnsAsync(true);
        _mediator.Setup(m => m.Send(It.IsAny<TagPostCommand>(), default)).ReturnsAsync(true);

        var result = await controller.Flag(789, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;
        var msgProp = value.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Contains("Sucessfully flagged the post and created a false_information tag on it.", msgProp.GetValue(value).ToString());
    }

    [Fact]
    public async Task Unlike_Returns_Unauthorized_If_User_Not_Authenticated()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var result = await controller.Unlike(25);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Unlike_Returns_BadRequest_If_User_Has_Not_Liked_Post()
    {
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

        var result = await controller.Unlike(25);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var msgProp = bad.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("You have not liked this post or already unliked it.", msgProp.GetValue(bad.Value));
    }

    [Fact]
    public async Task Unlike_Returns_Ok_If_Successful()
    {
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

        var result = await controller.Unlike(25);

        var ok = Assert.IsType<OkObjectResult>(result);
        var msgProp = ok.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Successfully removed like on the post.", msgProp.GetValue(ok.Value));
    }

    [Fact]
    public async Task Unlike_Returns_Unauthorized_If_UserId_Is_Zero()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new PostsController(mediatorMock.Object);

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        var result = await controller.Unlike(44);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
