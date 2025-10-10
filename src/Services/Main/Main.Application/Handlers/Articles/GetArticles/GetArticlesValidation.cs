using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Main.Core.Dtos.Anonymous.Articles;

namespace Main.Application.Handlers.Articles.GetArticles;

public class GetArticlesAnonymousValidation : AbstractValidator<GetArticlesQuery<ArticleDto>>
{
    public GetArticlesAnonymousValidation()
    {
        RuleFor(x => x.SearchTerm)
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина строки поиска — 3");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}

public class GetArticlesAmwValidation : AbstractValidator<GetArticlesQuery<global::Main.Core.Dtos.Amw.Articles.ArticleDto>>
{
    public GetArticlesAmwValidation()
    {
        RuleFor(x => x.SearchTerm)
            .Must(x => x.Trim().Length >= 3)
            .WithMessage("Минимальная длина строки поиска — 3");

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}