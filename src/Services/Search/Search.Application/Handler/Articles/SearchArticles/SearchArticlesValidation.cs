using FluentValidation;
using Localization.Domain.Extensions;

namespace Search.Application.Handler.Articles.SearchArticles;

public class SearchArticlesValidation : AbstractValidator<SearchArticlesQuery>
{
    public SearchArticlesValidation()
    {
        RuleFor(query => query.Query)
            .NotEmpty()
            .WithLocalizationKey("");
        
        RuleFor(query => query.Limit)
            .GreaterThan(0)
            .WithLocalizationKey("");
    }
}