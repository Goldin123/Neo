namespace Neo.Tests.Integration;

using Microsoft.Extensions.Logging.Abstractions;
using Neo.Domain.Entities;
using Neo.Domain.Enums;
using Neo.Infrastructure.Repositories;
using Xunit;

public class PostLikeRepositoryTests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;
    private readonly PostRepository _postRepo;
    private readonly PostLikeRepository _likeRepo;
    private readonly UserRepository _userRepo;

    public PostLikeRepositoryTests(DbFixture fixture)
    {
        _fixture = fixture;
        _postRepo = new PostRepository(_fixture.DbContext, NullLogger<PostRepository>.Instance);
        _likeRepo = new PostLikeRepository(_fixture.DbContext, NullLogger<PostLikeRepository>.Instance);
        _userRepo = new UserRepository(_fixture.DbContext, NullLogger<UserRepository>.Instance);
    }

    [Fact]
    public async Task AddLikeAsync_Should_Add_Like_And_Return_NewId()
    {
        // Arrange: Create two users and a post for user1
        var user1 = new User { Username = $"user1_{Guid.NewGuid():N}", PasswordHash = "pw1", Role = UserRole.User };
        var user2 = new User { Username = $"user2_{Guid.NewGuid():N}", PasswordHash = "pw2", Role = UserRole.User };
        var user1Id = await _userRepo.CreateAsync(user1);
        var user2Id = await _userRepo.CreateAsync(user2);

        var post = new Post { UserId = user1Id, Title = $"Post {Guid.NewGuid()}", Content = "Like me", CreatedAt = DateTime.UtcNow };
        var postId = await _postRepo.CreateAsync(post);

        // Act: user2 likes user1's post
        var likeId = await _likeRepo.AddLikeAsync(postId, user2Id);

        // Assert
        Assert.True(likeId > 0);
    }

    [Fact]
    public async Task AddLikeAsync_Should_Return_Minus2_If_User_Likes_Own_Post()
    {
        // Arrange: Create user and post
        var user = new User { Username = $"selflike_{Guid.NewGuid():N}", PasswordHash = "pw", Role = UserRole.User };
        var userId = await _userRepo.CreateAsync(user);
        var post = new Post { UserId = userId, Title = $"Self Like {Guid.NewGuid()}", Content = "Don't like yourself", CreatedAt = DateTime.UtcNow };
        var postId = await _postRepo.CreateAsync(post);

        // Act: user likes their own post
        var result = await _likeRepo.AddLikeAsync(postId, userId);

        // Assert
        Assert.Equal(-2, result);
    }

    [Fact]
    public async Task AddLikeAsync_Should_Return_Minus1_If_Already_Liked()
    {
        // Arrange: Create two users and a post for user1
        var user1 = new User { Username = $"ulike1_{Guid.NewGuid():N}", PasswordHash = "pw1", Role = UserRole.User };
        var user2 = new User { Username = $"ulike2_{Guid.NewGuid():N}", PasswordHash = "pw2", Role = UserRole.User };
        var user1Id = await _userRepo.CreateAsync(user1);
        var user2Id = await _userRepo.CreateAsync(user2);
        var post = new Post { UserId = user1Id, Title = $"Unique Like {Guid.NewGuid()}", Content = "Only one like", CreatedAt = DateTime.UtcNow };
        var postId = await _postRepo.CreateAsync(post);

        // Act: user2 likes, then likes again
        var firstLike = await _likeRepo.AddLikeAsync(postId, user2Id);
        var secondLike = await _likeRepo.AddLikeAsync(postId, user2Id);

        // Assert
        Assert.True(firstLike > 0);
        Assert.Equal(-1, secondLike);
    }
}
