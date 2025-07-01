namespace Neo.Tests.Integration;

using Microsoft.Extensions.Logging.Abstractions;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Repositories;
using Xunit;

public class PostRepositoryTests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;                
    private readonly IPostRepository _repository;

    public PostRepositoryTests(DbFixture fixture)
    {
        _fixture = fixture;                            
        _repository = new PostRepository(_fixture.DbContext, NullLogger<PostRepository>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Should_Insert_And_Get_Post_By_Id()
    {
        // Step 1: Create a user first
        var userRepo = new UserRepository(_fixture.DbContext, NullLogger<UserRepository>.Instance); 
        var uniqueUserName = $"testuser_{Guid.NewGuid():N}";
        var user = new Neo.Domain.Entities.User
        {
            Username = uniqueUserName,
            PasswordHash = "hashed_pw",
            Role = Neo.Domain.Enums.UserRole.User
        };
        var userId = await userRepo.CreateAsync(user);

        // Step 2: Create the post for that user
        var post = new Post
        {
            UserId = userId,
            Title = $"Integration test post {Guid.NewGuid()}",
            Content = "Test content",
            CreatedAt = DateTime.UtcNow
        };

        var postId = await _repository.CreateAsync(post);
        var fetched = await _repository.GetByIdAsync(postId);

        Assert.NotNull(fetched);
        Assert.Equal(post.Title, fetched!.Title);
        Assert.Equal(post.Content, fetched.Content);
        Assert.Equal(post.UserId, fetched.UserId);
    }

    // Other tests ...
}
