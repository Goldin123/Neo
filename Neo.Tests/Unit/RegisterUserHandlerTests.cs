using Microsoft.Extensions.Logging;
using Moq;
using Neo.Application.UseCases.RegisterUser;
using Neo.Domain.Entities;
using Neo.Domain.Enums;
using Neo.Domain.Interfaces;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> userRepo = new();
    private readonly Mock<IPasswordHasher> hasher = new();
    private readonly Mock<ILogger<RegisterUserHandler>> logger = new();
    private readonly RegisterUserHandler handler;

    public RegisterUserHandlerTests()
    {
        handler = new RegisterUserHandler(userRepo.Object, hasher.Object, logger.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Username_Exists()
    {
        userRepo.Setup(x => x.GetByUsernameAsync("test")).ReturnsAsync(new User());

        var result = await handler.Handle(new RegisterUserCommand("test", "pass", UserRole.Moderator), default);

        Assert.False(result.Success);
        Assert.Equal("Username already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_Should_Create_User_When_Username_Not_Exists()
    {
        userRepo.Setup(x => x.GetByUsernameAsync("new")).ReturnsAsync((User?)null);
        hasher.Setup(x => x.HashPassword("pass")).Returns("hashed");
        userRepo.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(42);

        var result = await handler.Handle(new RegisterUserCommand("new", "pass", UserRole.Moderator), default);

        Assert.True(result.Success);
        Assert.Equal(42, result.UserId);
    }
}
