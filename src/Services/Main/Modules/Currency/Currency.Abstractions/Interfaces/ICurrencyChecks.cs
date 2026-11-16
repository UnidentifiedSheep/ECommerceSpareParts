using BulkValidation.Core.Interfaces;

namespace Main.Modules.Currency.Abstractions.Interfaces;

public interface ICurrencyChecks
{
	IValidationPlan Exists(IValidationPlan plan, int currencyId);

	IValidationPlan Exists(IValidationPlan plan, IEnumerable<int> currencyIds);
}
