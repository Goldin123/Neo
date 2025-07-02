using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Neo.Api.Controllers;
using Neo.Application.UseCases.RegisterUser;
using Neo.Domain.Entities;
using Neo.Domain.Enums;
using Neo.Domain.Interfaces;
using Xunit;

namespace Neo.Tests.Functional;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();

    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(
            _mediator.Object,
            _userRepo.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object
        );
    }

    [Fact]
    public async Task Register_Returns_BadRequest_If_Fields_Are_Empty()
    {
        var dto = new AuthController.RegisterDto("", "", "");

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username, password, and role are required.", badRequest.Value);
    }

    [Fact]
    public async Task Register_Returns_BadRequest_If_Invalid_Role()
    {
        var dto = new AuthController.RegisterDto("user", "pass", "InvalidRole");

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        // Should match the anonymous object { message = "Invalid role." }
        var msgProp = badRequest.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Invalid role.", msgProp.GetValue(badRequest.Value));
    }

    [Fact]
    public async Task Register_Returns_Conflict_If_Registration_Fails()
    {
        var dto = new AuthController.RegisterDto("user", "pass", "Moderator");

        _mediator.Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), default))
                 .ReturnsAsync(new RegisterUserResult(false, null, "Username already exists."));

        var result = await _controller.Register(dto);

        var conflict = Assert.IsType<ConflictObjectResult>(result);

        // Use reflection to access anonymous `error` property
        var errorProp = conflict.Value!.GetType().GetProperty("error");
        Assert.NotNull(errorProp);
        Assert.Equal("Username already exists.", errorProp.GetValue(conflict.Value));
    }

    [Fact]
    public async Task Register_Returns_Ok_If_Registration_Succeeds()
    {
        var dto = new AuthController.RegisterDto("user", "pass", "Moderator");

        _mediator.Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), default))
                 .ReturnsAsync(new RegisterUserResult(true, 101));

        var result = await _controller.Register(dto);

        var ok = Assert.IsType<OkObjectResult>(result);

        // Use reflection to get anonymous `message` and `Id` values
        var messageProp = ok.Value!.GetType().GetProperty("message");
        var idProp = ok.Value!.GetType().GetProperty("Id");
        Assert.NotNull(messageProp);
        Assert.NotNull(idProp);

        Assert.Equal("Successfully registerd a user.", messageProp.GetValue(ok.Value));
        Assert.Equal(101, idProp.GetValue(ok.Value));
    }

    #region Login

    [Fact]
    public async Task Login_Returns_Ok_With_Token_If_Credentials_Valid()
    {
        var user = new User { Id = 1, Username = "user", PasswordHash = "hashed", Role = UserRole.Moderator };
        var dto = new AuthController.LoginDto("user", "password");

        _userRepo.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("password", "hashed")).Returns(true);
        _jwtTokenService.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await _controller.Login(dto);

        var ok = Assert.IsType<OkObjectResult>(result);

        var tokenProp = ok.Value!.GetType().GetProperty("token");
        var usernameProp = ok.Value!.GetType().GetProperty("username");
        var roleProp = ok.Value!.GetType().GetProperty("role");
        var userIdProp = ok.Value!.GetType().GetProperty("userId");

        Assert.Equal("jwt-token", tokenProp?.GetValue(ok.Value));
        Assert.Equal("user", usernameProp?.GetValue(ok.Value));
        Assert.Equal(UserRole.Moderator, Enum.Parse<UserRole>(roleProp?.GetValue(ok.Value)?.ToString() ?? ""));
        Assert.Equal(1, userIdProp?.GetValue(ok.Value));
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_If_User_Not_Found()
    {
        var dto = new AuthController.LoginDto("missing", "pw");

        _userRepo.Setup(r => r.GetByUsernameAsync("missing")).ReturnsAsync((User?)null);

        var result = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);

        var msgProp = unauthorized.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Invalid credentials.", msgProp.GetValue(unauthorized.Value));
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_If_Password_Invalid()
    {
        var user = new User { Id = 1, Username = "user", PasswordHash = "hash", Role = UserRole.Moderator };
        var dto = new AuthController.LoginDto("user", "wrong");

        _userRepo.Setup(r => r.GetByUsernameAsync("user")).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.VerifyPassword("wrong", "hash")).Returns(false);

        var result = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);

        var msgProp = unauthorized.Value!.GetType().GetProperty("message");
        Assert.NotNull(msgProp);
        Assert.Equal("Invalid credentials.", msgProp.GetValue(unauthorized.Value));
    }

    #endregion
}
