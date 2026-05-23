using Application.Common.Validators;
using FluentValidation;

namespace Search.Application.Handlers.Producers.SearchProducers;

public class SearchProducersValidation : AbstractValidator<SearchProducersQuery>
{
    public SearchProducersValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}
