namespace Neo.Application.UseCases.LikePost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="LikePostCommand"/>.
/// </summary>
public sealed class LikePostCommandValidator : AbstractValidator<LikePostCommand>
{
    public LikePostCommandValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
