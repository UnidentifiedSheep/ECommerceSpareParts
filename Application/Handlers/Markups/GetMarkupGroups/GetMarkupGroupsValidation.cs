using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Markups.GetMarkupGroups;

public class GetMarkupGroupsValidation : AbstractValidator<GetMarkupGroupsQuery>
{
    public GetMarkupGroupsValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
    }
}