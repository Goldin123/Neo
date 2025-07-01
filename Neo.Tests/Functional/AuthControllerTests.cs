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

    private record RegisterSuccessResponse(int id);

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
        Assert.Equal("Invalid role.", badRequest.Value);
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
        var errorValue = errorProp?.GetValue(conflict.Value)?.ToString();

        Assert.Equal("Username already exists.", errorValue);
    }

    [Fact]
    public async Task Register_Returns_Ok_If_Registration_Succeeds()
    {
        var dto = new AuthController.RegisterDto("user", "pass", "Moderator");

        _mediator.Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), default))
                 .ReturnsAsync(new RegisterUserResult(true, 101));

        var result = await _controller.Register(dto);

        var ok = Assert.IsType<OkObjectResult>(result);

        // Use reflection to get anonymous `id` value
        var idProp = ok.Value!.GetType().GetProperty("id");
        var idValue = idProp?.GetValue(ok.Value);

        Assert.Equal(101, idValue);
    }

}
