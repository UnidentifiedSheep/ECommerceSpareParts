using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Permissions.GetPermission;

public class GetPermissionsValidation : AbstractValidator<GetPermissionsQuery>
{
    public GetPermissionsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}