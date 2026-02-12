using Application.Common.Validators;
using FluentValidation;

namespace Pricing.Application.Handlers.Markups.GetMarkupGroups;

public class GetMarkupGroupsValidation : AbstractValidator<GetMarkupGroupsQuery>
{
    public GetMarkupGroupsValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
    }
}