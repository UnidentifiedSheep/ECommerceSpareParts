using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Permissions.GetPermission;

public class GetPermissionsValidation : AbstractValidator<GetPermissionsQuery>
{
    public GetPermissionsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}