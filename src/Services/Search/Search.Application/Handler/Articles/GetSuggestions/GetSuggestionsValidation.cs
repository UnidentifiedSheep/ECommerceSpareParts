using FluentValidation;
using Localization.Domain.Extensions;

namespace Search.Application.Handler.Articles.GetSuggestions;

public class GetSuggestionsValidation : AbstractValidator<GetSuggestionsQuery>
{
    public GetSuggestionsValidation()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithLocalizationKey("");
        
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithLocalizationKey("");
    }
}