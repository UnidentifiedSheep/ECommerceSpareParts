using Application.Common.Extensions;
using FluentValidation;

namespace Main.Modules.Currency.Application.NamedObjects;

public class CurrencySettingInputValidator : AbstractValidator<CurrencySettingInputData>
{
	public CurrencySettingInputValidator() => RuleFor(x => x.RateProvider).DefinedEnum();
}
