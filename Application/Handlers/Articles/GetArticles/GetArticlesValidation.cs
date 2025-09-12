using Application.Handlers.BaseValidators;
using Core.Dtos.Anonymous.Articles;
using FluentValidation;

namespace Application.Handlers.Articles.GetArticles;

public class GetArticlesAnonymousValidation : AbstractValidator<GetArticlesQuery<ArticleDto>>
{
    public GetArticlesAnonymousValidation()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Минимальная длина строки поиска — 3");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}

public class GetArticlesAmwValidation : AbstractValidator<GetArticlesQuery<Core.Dtos.Amw.Articles.ArticleDto>>
{
    public GetArticlesAmwValidation()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Минимальная длина строки поиска — 3");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}