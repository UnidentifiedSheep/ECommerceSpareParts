using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Auth.GetRoles;

public class GetRolesValidation : AbstractValidator<GetRolesQuery>
{
    public GetRolesValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(x => new PaginationValidator());
    }
}