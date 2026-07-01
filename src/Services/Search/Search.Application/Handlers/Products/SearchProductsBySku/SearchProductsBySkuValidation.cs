using Application.Common.Validators;
using FluentValidation;

namespace Search.Application.Handlers.Products.SearchProductsBySku;

public class SearchProductsBySkuValidation : AbstractValidator<SearchProductsBySkuQuery>
{
    public SearchProductsBySkuValidation()
    {
        RuleFor(s => s.Pagination)
            .SetValidator(new PaginationValidator());
    }
}