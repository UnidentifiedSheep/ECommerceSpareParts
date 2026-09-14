using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;

namespace Main.Application.Handlers.Currencies.CreateCurrency;

public class CreateCurrencyValidation : AbstractValidator<CreateCurrencyCommand>
{
	public CreateCurrencyValidation()
	{
		RuleFor(x => x.Code)
			.NotEmpty()
			.WithLocalizableError(CurrencyCodeNotEmptyMessage.Instance)
			.MaximumLength(26)
			.WithLocalizableError(CurrencyCodeMaxLengthMessage.Instance)
			.Must(x => x.Trim().Length >= 2)
			.WithLocalizableError(CurrencyCodeMinLengthMessage.Instance);

		RuleFor(x => x.Name)
			.NotEmpty()
			.WithLocalizableError(CurrencyNameNotEmptyMessage.Instance)
			.MaximumLength(128)
			.WithLocalizableError(CurrencyNameMaxLengthMessage.Instance)
			.Must(x => x.Trim().Length >= 3)
			.WithLocalizableError(CurrencyNameMinLengthMessage.Instance);

		RuleFor(x => x.CurrencySign)
			.NotEmpty()
			.WithLocalizableError(CurrencySignNotEmptyMessage.Instance)
			.MaximumLength(3)
			.WithLocalizableError(CurrencySignMaxLengthMessage.Instance)
			.Must(x => x.Trim().Length >= 1)
			.WithLocalizableError(CurrencySignMinLengthMessage.Instance);

		RuleFor(x => x.ShortName)
			.NotEmpty()
			.WithLocalizableError(CurrencyShortNameNotEmptyMessage.Instance)
			.MaximumLength(5)
			.WithLocalizableError(CurrencyShortNameMaxLengthMessage.Instance)
			.Must(x => x.Trim().Length >= 2)
			.WithLocalizableError(CurrencyShortNameMinLengthMessage.Instance);
	}
}
