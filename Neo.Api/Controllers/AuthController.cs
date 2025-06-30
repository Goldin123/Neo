using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo.Domain.Entities;
using Neo.Domain.Interfaces;
using Neo.Infrastructure.Auth;

namespace Neo.Api.Controllers;

/// <summary>
/// Handles authentication (login, register) operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserRepository userRepo,
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
        var hash = PasswordHasher.HashPassword(dto.Password);

        // Convert string to UserRole enum (case-insensitive)
        if (!Enum.TryParse<Neo.Domain.Enums.UserRole>(dto.Role, true, out var userRole))
            return BadRequest("Invalid role.");

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = hash,
            Role = userRole
        };

        var id = await userRepo.CreateAsync(user);
        return Ok(new { id });
    }

    /// <summary>
    /// Logs a user in and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await userRepo.GetByUsernameAsync(dto.Username);
        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = jwtTokenService.GenerateToken(user);
        return Ok(new { token, username = user.Username, role = user.Role, userId = user.Id });
    }

    // DTOs for login/register
    public record LoginDto(string Username, string Password);
    public record RegisterDto(string Username, string Password, string Role);
}
