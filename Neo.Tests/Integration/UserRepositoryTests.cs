using Microsoft.Extensions.Logging.Abstractions;
using Neo.Domain.Entities;
using Neo.Domain.Enums;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Repositories;
using Xunit;

namespace Neo.Tests.Integration;

public class UserRepositoryTests : IClassFixture<DbFixture>
{
    private readonly IUserRepository _repository;

    public UserRepositoryTests(DbFixture fixture)
    {
        _repository = new UserRepository(fixture.DbContext, NullLogger<UserRepository>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Should_Insert_And_Get_User_By_Id()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}";
        var user = new User
        {
            Username = username,
            PasswordHash = "hashed_123",
            Role = UserRole.Moderator
        };

        // Act
        var newId = await _repository.CreateAsync(user);
        var fetched = await _repository.GetByIdAsync(newId);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(username, fetched!.Username);
        Assert.Equal("hashed_123", fetched.PasswordHash);
        Assert.Equal(UserRole.Moderator, fetched.Role);
    }

    [Fact]
    public async Task GetByUsernameAsync_Should_Return_Existing_User()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}";
        var user = new User
        {
            Username = username,
            PasswordHash = "pw_hash",
            Role = UserRole.Moderator
        };
        var id = await _repository.CreateAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("pw_hash", result.PasswordHash);
        Assert.Equal(UserRole.Moderator, result.Role);
    }

    [Fact]
    public async Task UsernameExistsAsync_Should_Return_True_If_User_Exists()
    {
        // Arrange
        var username = $"check_{Guid.NewGuid():N}";
        await _repository.CreateAsync(new User
        {
            Username = username,
            PasswordHash = "any",
            Role = UserRole.Moderator
        });

        // Act
        var exists = await _repository.UsernameExistsAsync(username);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task UsernameExistsAsync_Should_Return_False_If_User_Does_Not_Exist()
    {
        // Arrange
        var username = $"missing_{Guid.NewGuid():N}";

        // Act
        var exists = await _repository.UsernameExistsAsync(username);

        // Assert
        Assert.False(exists);
    }
}
