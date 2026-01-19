using Application.Common.Abstractions;
using BulkValidation.Core.Interfaces;
using Main.Entities;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

public class CreateCurrencyDbValidation : AbstractDbValidation<CreateCurrencyCommand>
{
    public override void Build(IValidationPlan plan, CreateCurrencyCommand request)
    {
        plan.ValidateCurrencyNotExistsCode(request.Code)
            .ValidateCurrencyNotExistsName(request.Name)
            .ValidateCurrencyNotExistsShortName(request.ShortName)
            .ValidateCurrencyNotExistsCurrencySign(request.CurrencySign);
    }
}