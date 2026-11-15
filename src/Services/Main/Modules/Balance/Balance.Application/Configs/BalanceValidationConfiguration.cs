using BulkValidation.Core.Configuration;
using Main.Entities.Balance;
using Main.Entities.Exceptions;

namespace Main.Application.Configs;

public static class BalanceValidationConfiguration
{
	public static void Configure(ValidationErrors errors)
		=> errors
			.For(TransactionKeys.Id)
			.OnMissing(input => new TransactionNotFoundException(input.Values[0]));
}
