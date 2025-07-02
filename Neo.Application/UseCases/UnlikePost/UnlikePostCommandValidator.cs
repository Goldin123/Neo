namespace Neo.Application.UseCases.UnlikePost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="UnlikePostCommand"/>.
/// </summary>
public sealed class UnlikePostCommandValidator : AbstractValidator<UnlikePostCommand>
{
    public UnlikePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0)
            .WithMessage("PostId must be greater than zero.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
