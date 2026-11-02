using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

public class CreateCurrencyDbValidation : AbstractDbValidation<CreateCurrencyCommand>
{
	public override void Build(IValidationPlan plan, CreateCurrencyCommand request)
	{
		plan
			.ValidateCurrencyNotExistsCode(request.Code.Trim())
			.ValidateCurrencyNotExistsName(request.Name.Trim())
			.ValidateCurrencyNotExistsShortName(request.ShortName.Trim())
			.ValidateCurrencyNotExistsCurrencySign(request.CurrencySign.Trim());
	}
}
