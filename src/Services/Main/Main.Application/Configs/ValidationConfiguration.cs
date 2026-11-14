using BulkValidation.Core.Configuration;
using Main.Modules.Currency.Application.Configs;
using Main.Entities;
using Main.Entities.Exceptions;
using Main.Entities.Product;

namespace Main.Application.Configs;

public static class ValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(OrganizationKeys.Id)
			.OnMissing(input => new OrganizationNotFoundException(input.Values[0]));

		errors.For(OrganizationMemberKeys.PK)
			.OnMissing(input => new OrganizationMemberNotFoundException(input.Values));

		errors.For(OrganizationMemberKeys.PK)
			.OnAlreadyExists(input => new OrganizationMemberAlreadyExistsException(input.Values));

		errors.For(OrganizationKeys.SystemName)
			.OnAlreadyExists(input => new OrganizationSystemNameAlreadyTakenException(input.Values));

		errors.For(StorageContentKeys.Id)
			.OnMissing(input => new StorageContentNotFoundException(input.Values));

		errors.For(StorageOwnerKeys.PK)
			.OnAlreadyExists(input => new StorageOwnerAlreadyExistsException(input.Values));

		errors.For(StorageOwnerKeys.PK)
			.OnMissing(input => new StorageOwnerNotFoundException(
				input.Values[0].UserId,
				input.Values[0].StorageCode));

		errors.For(StorageRouteKeys.Id)
			.OnMissing(input => new StorageRouteNotFound(input.Values[0]));

		errors.For(StorageRouteKeys.FromTo)
			.OnMissing(input => new StorageRouteNotFound(
				input.Values[0].FromStorageCode,
				input.Values[0].ToStorageCode));

		errors.For(StorageRouteKeys.FromTo)
			.OnAlreadyExists(input => new StorageRouteAlreadyExistsException(input.Values));

		errors.For(CartKeys.PK)
			.OnMissing(input => new CartItemNotFoundException(input.Values[0].ProductId));

		errors.For(CartKeys.PK)
			.OnAlreadyExists(input => new CartItemAlreadyExistsException(input.Values));

		CurrencyValidationConfiguration.Configure(errors);

		errors.For(PermissionKeys.Name)
			.OnMissing(input => new PermissionNotFoundException(string.Join(", ", input.Values)));

		errors.For(PermissionKeys.Name)
			.OnAlreadyExists(input => new PermissionAlreadyExistsException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnMissing(input => new StorageNotFoundException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnAlreadyExists(input => new StorageCodeAlreadyTakenException(input.Values));

		errors.For(UserKeys.UserName.NormalizedValue)
			.OnAlreadyExists(input => new UserNameAlreadyTakenException(input.Values[0]));

		errors.For(UserEmailKeys.Email)
			.OnAlreadyExists(input => new UserEmailAlreadyInUseException(input.Values[0].Value));

		errors.For(RoleKeys.Name)
			.OnMissing(input => new RoleNotFoundException(input.Values[0]));

		errors.For(UserKeys.Id)
			.OnMissing(input => new UserNotFoundException(input.Values[0]));

		errors.For(TransactionKeys.Id)
			.OnMissing(input => new TransactionNotFoundException(input.Values[0]));

		ProductValidationConfiguration.Configure(errors);
	}
}
