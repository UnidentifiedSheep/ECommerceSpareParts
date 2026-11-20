using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Organization.Application.Queries.Organizations.GetOrganizationMembers;

public class GetOrganizationMembersValidation : AbstractValidator<GetOrganizationMembersQuery>
{
	public GetOrganizationMembersValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
