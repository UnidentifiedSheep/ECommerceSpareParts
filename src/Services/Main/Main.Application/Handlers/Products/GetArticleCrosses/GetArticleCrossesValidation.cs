using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetArticleCrossesAmwValidation : AbstractValidator<GetArticleCrossesQuery>
{
    public GetArticleCrossesAmwValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}