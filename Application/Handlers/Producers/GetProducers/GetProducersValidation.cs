using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Producers.GetProducers;

public class GetProducersValidation : AbstractValidator<GetProducersQuery>
{
    public GetProducersValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}