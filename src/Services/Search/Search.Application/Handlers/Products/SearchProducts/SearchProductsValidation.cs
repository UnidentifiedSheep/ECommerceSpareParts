using Application.Common.Validators;
using FluentValidation;

namespace Search.Application.Handlers.Products.SearchProducts;

public class SearchProductsValidation : AbstractValidator<SearchProductsQuery>
{
    public  SearchProductsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}