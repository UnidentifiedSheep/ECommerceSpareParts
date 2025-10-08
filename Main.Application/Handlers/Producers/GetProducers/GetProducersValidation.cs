using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Producers.GetProducers;

public class GetProducersValidation : AbstractValidator<GetProducersQuery>
{
    public GetProducersValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}