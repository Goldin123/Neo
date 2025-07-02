namespace Neo.Application.UseCases.AddComment;

using FluentValidation;

/// <summary>
/// Validates the <see cref="AddCommentCommand"/>.
/// </summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
    }
}
