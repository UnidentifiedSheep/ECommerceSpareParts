using Application.Common.Validators;
using Main.Entities.Organization;
using FluentValidation;

namespace Main.Application.Handlers.Organizations.GetOrganizationMembers;

public class GetOrganizationMembersValidation : AbstractValidator<GetOrganizationMembersQuery>
{
	public GetOrganizationMembersValidation()
		=> RuleFor(x => x.Pagination).SetValidator(new PaginationValidator());
}
