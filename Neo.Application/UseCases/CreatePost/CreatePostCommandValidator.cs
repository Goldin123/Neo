namespace Neo.Application.UseCases.CreatePost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="CreatePostCommand"/>.
/// </summary>
public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
    }
}
