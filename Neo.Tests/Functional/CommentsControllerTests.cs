using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Neo.Api.Controllers;
using Neo.Application.UseCases.AddComment;
using Neo.Application.UseCases.GetCommentsByPost;
using Neo.Domain.Entities;
using System.Security.Claims;
using Xunit;

namespace Neo.Tests.Functional;

public class CommentsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly CommentsController _controller;

    public CommentsControllerTests()
    {
        _controller = new CommentsController(_mediator.Object);
    }

    private void SetUser(CommentsController controller, int userId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetByPost_Returns_Ok_With_Comments()
    {
        // Arrange
        var postId = 11;
        var comments = new List<Comment>
        {
            new Comment { Id = 1, PostId = postId, UserId = 2, Content = "Hi", CreatedAt = DateTime.UtcNow }
        };
        _mediator.Setup(m => m.Send(It.Is<GetCommentsByPostQuery>(q => q.PostId == postId), default))
            .ReturnsAsync(comments);

        // Act
        var result = await _controller.GetByPost(postId);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Comment>>(ok.Value);
        Assert.Single(returned);
        Assert.Equal("Hi", returned.First().Content);
    }

    [Fact]
    public async Task Add_Returns_Unauthorized_If_UserId_Missing()
    {
        // Arrange
        var dto = new CommentsController.AddCommentDto(15, "Nice!");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Add(dto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Add_Returns_BadRequest_If_Content_Is_Empty()
    {
        // Arrange
        var dto = new CommentsController.AddCommentDto(23, "");
        SetUser(_controller, 7);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Comment content is required.", bad.Value);
    }

    [Fact]
    public async Task Add_Returns_Ok_With_CommentId_When_Successful()
    {
        // Arrange
        var userId = 7;
        var postId = 32;
        var dto = new CommentsController.AddCommentDto(postId, "Test comment");
        SetUser(_controller, userId);

        _mediator.Setup(m => m.Send(It.Is<AddCommentCommand>(cmd =>
            cmd.PostId == postId &&
            cmd.UserId == userId &&
            cmd.Content == dto.Content
        ), default)).ReturnsAsync(123);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var val = ok.Value!;
        var commentIdProp = val.GetType().GetProperty("commentId");
        Assert.NotNull(commentIdProp);
        Assert.Equal(123, commentIdProp.GetValue(val));
    }
}
