namespace Neo.Application.UseCases.GetPagedPosts;

using FluentValidation;

/// <summary>
/// Validates the <see cref="GetPagedPostsQuery"/>.
/// </summary>
public sealed class GetPagedPostsQueryValidator : AbstractValidator<GetPagedPostsQuery>
{
    public GetPagedPostsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        // Add more rules as needed
    }
}
