using BulkValidation.Core.Configuration;
using Main.Entities;
using Main.Entities.Exceptions;

namespace Main.Modules.Identity.Application.Configs;

public static class IdentityValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(PermissionKeys.Name)
			.OnMissing(input => new PermissionNotFoundException(string.Join(", ", input.Values)));

		errors.For(PermissionKeys.Name)
			.OnAlreadyExists(input => new PermissionAlreadyExistsException(string.Join(", ", input.Values)));

		errors.For(UserKeys.UserName.NormalizedValue)
			.OnAlreadyExists(input => new UserNameAlreadyTakenException(input.Values[0]));

		errors.For(UserEmailKeys.Email)
			.OnAlreadyExists(input => new UserEmailAlreadyInUseException(input.Values[0].Value));

		errors.For(RoleKeys.Name)
			.OnMissing(input => new RoleNotFoundException(input.Values[0]));

		errors.For(UserKeys.Id)
			.OnMissing(input => new UserNotFoundException(input.Values[0]));
	}
}
