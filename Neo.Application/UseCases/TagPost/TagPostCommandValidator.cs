namespace Neo.Application.UseCases.TagPost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="TagPostCommand"/>.
/// </summary>
public sealed class TagPostCommandValidator : AbstractValidator<TagPostCommand>
{
    public TagPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0)
            .WithMessage("PostId must be greater than 0.");

        RuleFor(x => x.TagName)
            .NotEmpty()
            .WithMessage("Tag name is required.")
            .MaximumLength(100)
            .WithMessage("Tag name must not exceed 100 characters.");
    }
}
