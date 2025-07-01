using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Neo.Application.UseCases.FlagPost;
using Neo.Domain.Interfaces;

namespace Neo.Tests.Unit;
public class FlagPostHandlerTests
{
    private readonly Mock<IPostRepository> _postRepoMock;
    private readonly Mock<ILogger<FlagPostHandler>> _loggerMock;
    private readonly FlagPostHandler _handler;

    public FlagPostHandlerTests()
    {
        _postRepoMock = new Mock<IPostRepository>();
        _loggerMock = new Mock<ILogger<FlagPostHandler>>();
        _handler = new FlagPostHandler(_postRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Moderator_CanFlagPost_AsMisleading_ReturnsTrue()
    {
        // Arrange
        var moderatorId = 101; // Assume moderator id
        var command = new FlagPostCommand(1, "misleading", moderatorId);

        _postRepoMock
            .Setup(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _postRepoMock.Verify(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId), Times.Once);
    }

    [Fact]
    public async Task Moderator_CanFlagPost_AsFalseInformation_ReturnsTrue()
    {
        // Arrange
        var moderatorId = 102; // Assume moderator id
        var command = new FlagPostCommand(2, "false_information", moderatorId);

        _postRepoMock
            .Setup(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _postRepoMock.Verify(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId), Times.Once);
    }

    [Fact]
    public async Task RegularUser_FlagsPost_HandlerProcessesRequest_ReturnsTrue()
    {
        // NOTE: Role enforcement is handled at the API/controller layer, not the handler.
        // This test demonstrates that the handler does not know the user role.
        var regularUserId = 200; // Assume regular user id
        var command = new FlagPostCommand(3, "misleading", regularUserId);

        _postRepoMock
            .Setup(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _postRepoMock.Verify(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId), Times.Once);
    }

    [Fact]
    public async Task Moderator_FlaggingFails_ReturnsFalse()
    {
        var moderatorId = 103;
        var command = new FlagPostCommand(4, "misleading", moderatorId);

        _postRepoMock
            .Setup(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _postRepoMock.Verify(r => r.FlagPostAsync(command.PostId, command.Reason, command.ModeratorId), Times.Once);
    }
}
