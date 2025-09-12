using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Storages.GetStorage;

public class GetStoragesValidation : AbstractValidator<GetStoragesQuery>
{
    public GetStoragesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}