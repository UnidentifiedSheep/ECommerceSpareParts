using Application.Common.Validators;
using FluentValidation;

namespace Main.Modules.Identity.Application.Queries.Auth.GetRoles;

public class GetRolesValidation : AbstractValidator<GetRolesQuery>
{
	public GetRolesValidation() => RuleFor(x => x.Pagination).SetValidator(x => new PaginationValidator());
}
