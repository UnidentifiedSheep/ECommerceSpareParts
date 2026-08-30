using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public class GetArticleCrossesAmwValidation : AbstractValidator<GetProductCrossesQuery>
{
    public GetArticleCrossesAmwValidation()
    {
        RuleForEach(x => x.Items)
            .ChildRules(item =>
                item.RuleFor(x => x.Pagination)
                    .SetValidator(new PaginationValidator()));
    }
}
