using BulkValidation.Core.Interfaces;
using Main.Entities.Currency;
using Main.Modules.Currency.Abstractions.Interfaces;

namespace Main.Modules.Currency.Application.Services;

public sealed class CurrencyChecks : ICurrencyChecks
{
	public IValidationPlan Exists(IValidationPlan plan, int currencyId)
		=> plan.ValidateCurrencyExistsId(currencyId);

	public IValidationPlan Exists(IValidationPlan plan, IEnumerable<int> currencyIds)
		=> plan.ValidateCurrencyExistsId(currencyIds);
}
