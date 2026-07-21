using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProducerAliases.GetProducerAliases;

public class GetProducerAliasesValidation : AbstractValidator<GetProducerAliasesQuery>
{
    public GetProducerAliasesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}