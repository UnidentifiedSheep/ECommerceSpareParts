using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Producers.GetProducerOtherNames;

public class GetProducerOtherNamesValidation : AbstractValidator<GetProducerOtherNamesQuery>
{
    public GetProducerOtherNamesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}