using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Markups.GetMarkupGroups;

public class GetMarkupGroupsValidation : AbstractValidator<GetMarkupGroupsQuery>
{
    public GetMarkupGroupsValidation()
    {
        RuleFor(query => query.Pagination)
            .SetValidator(new PaginationValidator());
    }
}