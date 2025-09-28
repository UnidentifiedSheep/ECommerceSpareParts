using Application.Handlers.BaseValidators;
using FluentValidation;

namespace Application.Handlers.Roles.GetRoles;

public class GetRolesValidation : AbstractValidator<GetRolesQuery>
{
    public GetRolesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(x => new PaginationValidator());
    }
}