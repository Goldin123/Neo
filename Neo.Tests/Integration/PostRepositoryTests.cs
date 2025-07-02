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

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_Post_Does_Not_Exist()
    {
        var fetched = await _repository.GetByIdAsync(-12345);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetPagedAsync_Should_Return_Inserted_Posts()
    {
        // Arrange: create a user and two posts
        var userRepo = new UserRepository(_fixture.DbContext, NullLogger<UserRepository>.Instance);
        var user = new Neo.Domain.Entities.User
        {
            Username = $"pageduser_{Guid.NewGuid():N}",
            PasswordHash = "pw",
            Role = Neo.Domain.Enums.UserRole.User
        };
        var userId = await userRepo.CreateAsync(user);

        var postsToInsert = new List<Post>
        {
            new Post { UserId = userId, Title = $"Paged Post A {Guid.NewGuid()}", Content = "Content A", CreatedAt = DateTime.UtcNow },
            new Post { UserId = userId, Title = $"Paged Post B {Guid.NewGuid()}", Content = "Content B", CreatedAt = DateTime.UtcNow }
        };

        foreach (var p in postsToInsert)
            await _repository.CreateAsync(p);

        // Act
        var pagedPosts = await _repository.GetPagedAsync(page: 1, pageSize: 10, authorId: userId);

        // Assert
        Assert.NotNull(pagedPosts);
        var titles = pagedPosts.Select(p => p.Title).ToList();
        Assert.Contains(postsToInsert[0].Title, titles);
        Assert.Contains(postsToInsert[1].Title, titles);
    }

    [Fact]
    public async Task FlagPostAsync_Should_Set_IsFlagged_And_Reason()
    {
        // Arrange: create user and post
        var userRepo = new UserRepository(_fixture.DbContext, NullLogger<UserRepository>.Instance);
        var user = new Neo.Domain.Entities.User
        {
            Username = $"moderator_{Guid.NewGuid():N}",
            PasswordHash = "pw",
            Role = Neo.Domain.Enums.UserRole.Moderator
        };
        var moderatorId = await userRepo.CreateAsync(user);

        var post = new Post
        {
            UserId = moderatorId, // Could be a different user's post in a real case
            Title = $"Flagged Post {Guid.NewGuid()}",
            Content = "Flag this content",
            CreatedAt = DateTime.UtcNow
        };

        var postId = await _repository.CreateAsync(post);

        // Act: flag the post
        var reason = "misleading";
        var flagResult = await _repository.FlagPostAsync(postId, reason, moderatorId);

        // Assert: flag operation succeeded
        Assert.True(flagResult);

        // Fetch and assert flagged status
        var fetched = await _repository.GetByIdAsync(postId);
        Assert.NotNull(fetched);
        Assert.True(fetched!.IsFlagged);
        Assert.Equal(reason, fetched.FlagReason);
    }

}
