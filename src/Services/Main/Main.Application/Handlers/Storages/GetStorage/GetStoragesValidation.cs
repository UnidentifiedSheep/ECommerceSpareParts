using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Storages.GetStorage;

public class GetStoragesValidation : AbstractValidator<GetStoragesQuery>
{
    public GetStoragesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}