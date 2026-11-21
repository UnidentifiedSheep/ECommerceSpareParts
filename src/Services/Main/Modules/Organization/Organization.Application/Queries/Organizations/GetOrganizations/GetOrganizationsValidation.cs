using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Organization.Application.Queries.Organizations.GetOrganizations;

public class GetOrganizationsValidation : AbstractValidator<GetOrganizationsQuery>
{
	public GetOrganizationsValidation() => RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
