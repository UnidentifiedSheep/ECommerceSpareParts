using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Auth.UpsertRole;

public class UpsertRoleValidation : AbstractValidator<UpsertRoleCommand>
{
	public UpsertRoleValidation()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithLocalizableError(RoleNameNotEmptyMessage.Instance)
			.Must(x => x.Trim().Length >= 3)
			.WithLocalizableError(RoleNameMinLengthMessage.Instance)
			.Must(x => x.Trim().Length <= 24)
			.WithLocalizableError(RoleNameMaxLengthMessage.Instance);
	}
}
