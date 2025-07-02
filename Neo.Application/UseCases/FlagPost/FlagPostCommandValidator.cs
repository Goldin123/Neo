namespace Neo.Application.UseCases.FlagPost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="FlagPostCommand"/>.
/// </summary>
public sealed class FlagPostCommandValidator : AbstractValidator<FlagPostCommand>
{
    public FlagPostCommandValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModeratorId).GreaterThan(0);
    }
}
