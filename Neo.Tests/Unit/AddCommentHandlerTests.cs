using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Neo.Application.UseCases.AddComment;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;

namespace Neo.Tests.Unit;

public class AddCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<ILogger<AddCommentHandler>> _loggerMock;
    private readonly AddCommentHandler _handler;

    public AddCommentHandlerTests()
    {
        _commentRepoMock = new Mock<ICommentRepository>();
        _loggerMock = new Mock<ILogger<AddCommentHandler>>();
        _handler = new AddCommentHandler(_commentRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Comment_And_Return_Id()
    {
        // Arrange
        var command = new AddCommentCommand(101, 12, "This is a comment!");

        // Simulate repo returning a new comment ID
        _commentRepoMock
            .Setup(r => r.CreateAsync(It.Is<Comment>(c =>
                c.PostId == command.PostId &&
                c.UserId == command.UserId &&
                c.Content == command.Content)))
            .ReturnsAsync(55);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(55, result);
        _commentRepoMock.Verify(r => r.CreateAsync(It.Is<Comment>(c =>
            c.PostId == command.PostId &&
            c.UserId == command.UserId &&
            c.Content == command.Content)), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Correct_Comment_Data_To_Repository()
    {
        // Arrange
        var command = new AddCommentCommand(200, 99, "Another comment!");
        Comment captured = null!;

        _commentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => captured = c)
            .ReturnsAsync(88);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(88, result);
        Assert.NotNull(captured);
        Assert.Equal(command.PostId, captured.PostId);
        Assert.Equal(command.UserId, captured.UserId);
        Assert.Equal(command.Content, captured.Content);
        Assert.True((DateTime.UtcNow - captured.CreatedAt).TotalSeconds < 5); // Timestamp is set
    }
}
