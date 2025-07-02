using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Neo.Application.UseCases.LikePost;
using Neo.Domain.Interfaces;
using Xunit;

namespace Neo.Tests.Unit;

public class LikePostHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_LikeId_When_Like_Succeeds()
    {
        // Arrange
        var repoMock = new Mock<IPostLikeRepository>();
        repoMock.Setup(r => r.AddLikeAsync(100, 50)).ReturnsAsync(77);

        var loggerMock = new Mock<ILogger<LikePostHandler>>();
        var handler = new LikePostHandler(repoMock.Object, loggerMock.Object);
        var command = new LikePostCommand(PostId: 100, UserId: 50);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(77, result);
        repoMock.Verify(r => r.AddLikeAsync(100, 50), Times.Once);
    }
}
