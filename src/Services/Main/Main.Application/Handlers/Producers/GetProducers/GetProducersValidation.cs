using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Producers.GetProducers;

public class GetProducersValidation : AbstractValidator<GetProducersQuery>
{
    public GetProducersValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}