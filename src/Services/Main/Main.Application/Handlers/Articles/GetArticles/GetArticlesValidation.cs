using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using ArticleDto = Main.Abstractions.Dtos.Anonymous.Articles.ArticleDto;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleDto;

namespace Main.Application.Handlers.Articles.GetArticles;

public class GetArticlesAnonymousValidation : AbstractValidator<GetArticlesQuery<ArticleDto>>
{
    public GetArticlesAnonymousValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}

public class GetArticlesAmwValidation : AbstractValidator<GetArticlesQuery<AmwArticleDto>>
{
    public GetArticlesAmwValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}