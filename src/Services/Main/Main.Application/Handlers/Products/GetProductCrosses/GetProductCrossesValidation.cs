using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetArticleCrossesAmwValidation : AbstractValidator<GetProductCrossesQuery>
{
    public GetArticleCrossesAmwValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}