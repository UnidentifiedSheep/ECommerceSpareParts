using BulkValidation.Core.Configuration;
using Main.Entities;
using Main.Entities.Exceptions;

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
			.OnMissing(input => new StorageOwnerNotFoundException(input.Values[0].UserId, input.Values[0].StorageCode));

		errors.For(StorageRouteKeys.Id)
			.OnMissing(input => new StorageRouteNotFound(input.Values[0]));

		errors.For(StorageRouteKeys.FromTo)
			.OnMissing(input => new StorageRouteNotFound(input.Values[0].FromStorageCode, input.Values[0].ToStorageCode));

		errors.For(StorageRouteKeys.FromTo)
			.OnAlreadyExists(input => new StorageRouteAlreadyExistsException(input.Values));

		errors.For(CartKeys.PK)
			.OnMissing(input => new CartItemNotFoundException(input.Values[0].ProductId));

		errors.For(CartKeys.PK)
			.OnAlreadyExists(input => new CartItemAlreadyExistsException(input.Values));

		errors.For(ProducerAliasKeys.Alias)
			.OnMissing(input => new ProducersAliasNotFoundException(string.Join(", ", input.Values)));

		errors.For(ProducerAliasKeys.Alias)
			.OnAlreadyExists(input => new ProducerAliasAlreadyTakenException(input.Values));

		errors.For(CurrencyKeys.Id)
			.OnMissing(input => new CurrencyNotFoundException(input.Values));

		errors.For(CurrencyKeys.Code)
			.OnMissing(input => new CurrencyCodeNotFoundException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.Code)
			.OnAlreadyExists(input => new CurrencyCodeAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.Name)
			.OnAlreadyExists(input => new CurrencyNameAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.ShortName)
			.OnAlreadyExists(input => new CurrencyShortNameAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.CurrencySign)
			.OnAlreadyExists(input => new CurrencySignAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(PermissionKeys.Name)
			.OnMissing(input => new PermissionNotFoundException(string.Join(", ", input.Values)));

		errors.For(PermissionKeys.Name)
			.OnAlreadyExists(input => new PermissionAlreadyExistsException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnMissing(input => new StorageNotFoundException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnAlreadyExists(input => new StorageCodeAlreadyTakenException(input.Values));

		errors.For(ProductKeys.Id)
			.OnMissing(input => input.IsBatch
				? new ProductNotFoundException(input.Values)
				: new ProductNotFoundException(input.Values[0]));

		errors.For(ProductGroupKeys.NormalizedName)
			.OnAlreadyExists(input => new ProductGroupNameAlreadyExistsException());

		errors.For(ProducerKeys.Id)
			.OnMissing(input => new ProducerNotFoundException(input.Values[0]));

		errors.For(ProducerKeys.Name)
			.OnMissing(input => new ProducerNameNotFoundException(input.Values));

		errors.For(UserKeys.Id)
			.OnMissing(input => new UserNotFoundException(input.Values[0]));

		errors.For(TransactionKeys.Id)
			.OnMissing(input => new TransactionNotFoundException(input.Values[0]));
	}
}
