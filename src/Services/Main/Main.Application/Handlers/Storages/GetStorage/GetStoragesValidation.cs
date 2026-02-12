using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Storages.GetStorage;

public class GetStoragesValidation : AbstractValidator<GetStoragesQuery>
{
    public GetStoragesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}