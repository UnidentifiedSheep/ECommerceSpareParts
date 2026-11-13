using BulkValidation.Core.Configuration;
using Main.Entities.Currency;
using Main.Entities.Exceptions;

namespace Main.Modules.Currency.Application.Configs;

public static class CurrencyValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
	{
		errors.For(CurrencyKeys.Id)
			.OnMissing(input => new CurrencyNotFoundException(input.Values));

		errors.For(CurrencyKeys.Code)
			.OnMissing(input => new CurrencyCodeNotFoundException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.Code)
			.OnAlreadyExists(input => new CurrencyCodeAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.Name)
			.OnAlreadyExists(input => new CurrencyNameAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.ShortName)
			.OnAlreadyExists(input
				=> new CurrencyShortNameAlreadyTakenException(string.Join(", ", input.Values)));

		errors.For(CurrencyKeys.CurrencySign)
			.OnAlreadyExists(input => new CurrencySignAlreadyTakenException(string.Join(", ", input.Values)));
	}
}
