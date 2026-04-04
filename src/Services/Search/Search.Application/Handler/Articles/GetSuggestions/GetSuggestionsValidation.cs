using FluentValidation;
using Localization.Domain.Extensions;

namespace Search.Application.Handler.Articles.GetSuggestions;

public class GetSuggestionsValidation : AbstractValidator<GetSuggestionsQuery>
{
    public GetSuggestionsValidation()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithLocalizationKey("suggestion.search.query.empty");
        
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithLocalizationKey("suggestion.search.limit.min");
    }
}