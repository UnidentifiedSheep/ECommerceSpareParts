using BulkValidation.Core.Configuration;
using Main.Entities.Storage;
using Main.Entities.Storage.Exceptions;

namespace Main.Modules.Storage.Application.Configs;

public static class StorageValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(StorageContentKeys.Id)
			.OnMissing(input => new StorageContentNotFoundException(input.Values));

		errors.For(StorageKeys.Code)
			.OnMissing(input => new StorageNotFoundException(string.Join(", ", input.Values)));

		errors.For(StorageKeys.Code)
			.OnAlreadyExists(input => new StorageCodeAlreadyTakenException(input.Values));
	}
}
