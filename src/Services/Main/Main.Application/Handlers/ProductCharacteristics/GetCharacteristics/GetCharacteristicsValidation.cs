using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;

public class GetCharacteristicsValidation : AbstractValidator<GetCharacteristicsQuery>
{
    public GetCharacteristicsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}