namespace Neo.Application.UseCases.GetCommentsByPost;

using FluentValidation;

/// <summary>
/// Validates the <see cref="GetCommentsByPostQuery"/>.
/// </summary>
public sealed class GetCommentsByPostQueryValidator : AbstractValidator<GetCommentsByPostQuery>
{
    public GetCommentsByPostQueryValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0);
    }
}
