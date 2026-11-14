using BulkValidation.Core.Configuration;
using Main.Entities.Exceptions;
using Main.Entities.Storage;

namespace Main.Application.Configs;

public static class StorageValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
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

		errors.For(StorageKeys.Code)
			.OnMissing(input => new StorageNotFoundException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnAlreadyExists(input => new StorageCodeAlreadyTakenException(input.Values));
	}
}
