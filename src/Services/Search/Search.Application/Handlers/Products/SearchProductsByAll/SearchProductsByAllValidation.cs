using Application.Common.Validators;
using FluentValidation;

namespace Search.Application.Handlers.Products.SearchProductsByAll;

public class SearchProductsByAllValidation : AbstractValidator<SearchProductsByAllQuery>
{
    public SearchProductsByAllValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}