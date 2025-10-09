using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Roles.GetRoles;

public class GetRolesValidation : AbstractValidator<GetRolesQuery>
{
    public GetRolesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(x => new PaginationValidator());
    }
}