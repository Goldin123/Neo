using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Application.UseCases.RegisterUser;
using Neo.Domain.Interfaces;

namespace Neo.Api.Controllers;

/// <summary>
/// Handles authentication (login, register) operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IMediator mediator,
    IUserRepository userRepo,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService
) : ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Optionally validate role format here, if it's an enum:
        // (otherwise let validation handle it in the Application layer)
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Username, password, and role are required.");

        // Convert string to UserRole enum (case-insensitive)
        if (!Enum.TryParse<Neo.Domain.Enums.UserRole>(dto.Role, true, out var userRole))
            return BadRequest("Invalid role.");
        // If you want, you can add a validator for RegisterUserCommand to ensure valid role.

        // Send command to application layer via MediatR
        var result = await mediator.Send(new RegisterUserCommand(dto.Username, dto.Password, userRole));
        if (!result.Success)
            return Conflict(new { error = result.ErrorMessage });
        return Ok(new { id = result.UserId });
    }

    /// <summary>
    /// Logs a user in and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userRepo.GetByUsernameAsync(dto.Username);
        if (user == null || !passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = jwtTokenService.GenerateToken(user);
        return Ok(new { token, username = user.Username, role = user.Role, userId = user.Id });
    }

    // DTOs for login/register
    public record LoginDto(string Username, string Password);
    public record RegisterDto(string Username, string Password, string Role);
}
