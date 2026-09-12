using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class NewSaleContentValidator : AbstractValidator<IEnumerable<NewSaleContentDto>>
{
	public NewSaleContentValidator()
	{
		RuleForEach(x => x)
			.ChildRules(z =>
			{
				z.RuleFor(x => x.Count).SetValidator(new CountValidator());

				z
					.RuleFor(x => x.Price)
					.GreaterThan(0)
					.WithLocalizableError(SaleContentPriceMinMessage.Instance)
					.PrecisionScale(
						18,
						2,
						true)
					.WithLocalizableError(SaleContentPricePrecisionMessage.Instance);

				z
					.RuleFor(x => x.PriceWithDiscount)
					.GreaterThan(0)
					.WithLocalizableError(SaleContentPriceWithDiscountMinMessage.Instance)
					.PrecisionScale(
						18,
						2,
						true)
					.WithLocalizableError(SaleContentPriceWithDiscountPrecisionMessage.Instance);

				z
					.RuleFor(x => x.PriceWithDiscount)
					.LessThanOrEqualTo(x => x.Price)
					.WithLocalizableError(SaleContentPriceWithDiscountMaxMessage.Instance);
			});

		RuleFor(x => x).NotEmpty().WithLocalizableError(SaleContentListNotEmptyMessage.Instance);
	}
}
