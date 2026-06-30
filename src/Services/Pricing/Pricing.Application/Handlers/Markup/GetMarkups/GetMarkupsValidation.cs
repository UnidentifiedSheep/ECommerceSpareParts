using Application.Common.Validators;
using FluentValidation;

namespace Pricing.Application.Handlers.Markup.GetMarkups;

public class GetMarkupsValidation : AbstractValidator<GetMarkupsQuery>
{
    public GetMarkupsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}