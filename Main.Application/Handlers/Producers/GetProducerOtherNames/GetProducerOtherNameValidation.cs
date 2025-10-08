using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Producers.GetProducerOtherNames;

public class GetProducerOtherNamesValidation : AbstractValidator<GetProducerOtherNamesQuery>
{
    public GetProducerOtherNamesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}