using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Neo.Application.UseCases.UnlikePost;
using Neo.Domain.Interfaces;

namespace Neo.Tests.Unit;
public class UnlikePostHandlerTests
{
    private readonly Mock<IPostLikeRepository> _likeRepoMock;
    private readonly Mock<ILogger<UnlikePostHandler>> _loggerMock;
    private readonly UnlikePostHandler _handler;

    public UnlikePostHandlerTests()
    {
        _likeRepoMock = new Mock<IPostLikeRepository>();
        _loggerMock = new Mock<ILogger<UnlikePostHandler>>();
        _handler = new UnlikePostHandler(_likeRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturn1_WhenLikeIsRemoved()
    {
        // Arrange
        var command = new UnlikePostCommand(101, 55);
        _likeRepoMock
            .Setup(r => r.RemoveLikeAsync(command.PostId, command.UserId))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);
        _likeRepoMock.Verify(r => r.RemoveLikeAsync(command.PostId, command.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnMinus1_WhenLikeDoesNotExist()
    {
        // Arrange
        var command = new UnlikePostCommand(101, 55);
        _likeRepoMock
            .Setup(r => r.RemoveLikeAsync(command.PostId, command.UserId))
            .ReturnsAsync(-1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(-1, result);
        _likeRepoMock.Verify(r => r.RemoveLikeAsync(command.PostId, command.UserId), Times.Once);
    }
}
