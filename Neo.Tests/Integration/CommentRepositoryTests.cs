using Microsoft.Extensions.Logging.Abstractions;
using Neo.Domain.Entities;
using Neo.Domain.Enums;
using Neo.Infrastructure.Repositories;
using Xunit;

namespace Neo.Tests.Integration;

public class CommentRepositoryTests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;
    private readonly CommentRepository _commentRepo;
    private readonly UserRepository _userRepo;
    private readonly PostRepository _postRepo;

    public CommentRepositoryTests(DbFixture fixture)
    {
        _fixture = fixture;
        _commentRepo = new CommentRepository(_fixture.DbContext, NullLogger<CommentRepository>.Instance);
        _userRepo = new UserRepository(_fixture.DbContext, NullLogger<UserRepository>.Instance);
        _postRepo = new PostRepository(_fixture.DbContext, NullLogger<PostRepository>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Should_Insert_And_Get_Comment_By_Id()
    {
        // Arrange: create a user and a post
        var user = new User { Username = $"commenter_{Guid.NewGuid():N}", PasswordHash = "pw", Role = UserRole.User };
        var userId = await _userRepo.CreateAsync(user);
        var post = new Post { UserId = userId, Title = $"Post for comment {Guid.NewGuid()}", Content = "Test content", CreatedAt = DateTime.UtcNow };
        var postId = await _postRepo.CreateAsync(post);

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = "Integration test comment!",
            CreatedAt = DateTime.UtcNow
        };

        // Act: create and fetch the comment
        var commentId = await _commentRepo.CreateAsync(comment);
        var fetched = await _commentRepo.GetByIdAsync(commentId);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(comment.PostId, fetched!.PostId);
        Assert.Equal(comment.UserId, fetched.UserId);
        Assert.Equal(comment.Content, fetched.Content);
        Assert.True((fetched.CreatedAt - comment.CreatedAt).TotalMinutes < 1);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_If_Not_Found()
    {
        var fetched = await _commentRepo.GetByIdAsync(-77777);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetByPostIdAsync_Should_Return_All_Comments_For_Post()
    {
        // Arrange: create user, post, and two comments
        var user = new User { Username = $"commented_{Guid.NewGuid():N}", PasswordHash = "pw", Role = UserRole.User };
        var userId = await _userRepo.CreateAsync(user);
        var post = new Post { UserId = userId, Title = $"Post for multiple comments {Guid.NewGuid()}", Content = "Test content", CreatedAt = DateTime.UtcNow };
        var postId = await _postRepo.CreateAsync(post);

        var comment1 = new Comment { PostId = postId, UserId = userId, Content = "First!", CreatedAt = DateTime.UtcNow };
        var comment2 = new Comment { PostId = postId, UserId = userId, Content = "Second!", CreatedAt = DateTime.UtcNow };
        await _commentRepo.CreateAsync(comment1);
        await _commentRepo.CreateAsync(comment2);

        // Act
        var comments = (await _commentRepo.GetByPostIdAsync(postId)).ToList();

        // Assert
        Assert.True(comments.Count >= 2);
        Assert.Contains(comments, c => c.Content == "First!");
        Assert.Contains(comments, c => c.Content == "Second!");
    }
}
