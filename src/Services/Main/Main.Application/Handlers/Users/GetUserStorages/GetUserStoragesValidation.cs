using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Users.GetUserStorages;

public class GetUserStoragesValidation : AbstractValidator<GetUserStoragesQuery>
{
    public GetUserStoragesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}