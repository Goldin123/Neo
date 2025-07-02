namespace Neo.Application.UseCases.RegisterUser;

using MediatR;
using Neo.Domain.Enums;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterUserCommand(string Username, string Password, UserRole Role) : IRequest<RegisterUserResult>;

/// <summary>
/// Result for registering a user.
/// </summary>
public record RegisterUserResult(bool Success, int? UserId, string? ErrorMessage = null);
