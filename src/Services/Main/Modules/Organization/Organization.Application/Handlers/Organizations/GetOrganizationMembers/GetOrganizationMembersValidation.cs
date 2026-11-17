using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Organization.Application.Handlers.Organizations.GetOrganizationMembers;

public class GetOrganizationMembersValidation : AbstractValidator<GetOrganizationMembersQuery>
{
	public GetOrganizationMembersValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
