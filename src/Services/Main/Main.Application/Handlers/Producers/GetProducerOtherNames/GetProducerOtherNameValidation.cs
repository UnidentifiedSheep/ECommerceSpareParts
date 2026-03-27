using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Producers.GetProducerOtherNames;

public class GetProducerOtherNamesValidation : AbstractValidator<GetProducerOtherNamesQuery>
{
    public GetProducerOtherNamesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}