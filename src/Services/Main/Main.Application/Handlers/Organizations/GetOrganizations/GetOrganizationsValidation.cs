using Application.Common.Validators;
using FluentValidation;

namespace Main.Application.Handlers.Organizations.GetOrganizations;

public class GetOrganizationsValidation : AbstractValidator<GetOrganizationsQuery>
{
    public GetOrganizationsValidation()
    {
        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationValidator());
    }
}
