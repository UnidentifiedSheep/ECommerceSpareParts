using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities.Sale;
using Main.Modules.Sale.Application.Dtos.Sale;

namespace Main.Modules.Sale.Application.Commands.Sales.BaseValidators;

public class NewSaleContentValidator : AbstractValidator<IReadOnlyCollection<NewSaleContentDto>>
{
	public NewSaleContentValidator()
	{
		RuleForEach(x => x)
			.ChildRules(z =>
			{
				z.RuleFor(x => x.Count).SetValidator(new CountValidator());

				z
					.RuleFor(x => x.OriginalPrice)
					.GreaterThan(0)
					.WithLocalizableError(SaleContentPriceMinMessage.Instance)
					.PrecisionScale(
						18,
						2,
						true)
					.WithLocalizableError(SaleContentPricePrecisionMessage.Instance);

				z
					.RuleFor(x => x.DiscountedPrice)
					.GreaterThan(0)
					.WithLocalizableError(SaleContentPriceWithDiscountMinMessage.Instance)
					.PrecisionScale(
						18,
						2,
						true)
					.WithLocalizableError(SaleContentPriceWithDiscountPrecisionMessage.Instance);

				z
					.RuleFor(x => x.DiscountedPrice)
					.LessThanOrEqualTo(x => x.OriginalPrice)
					.WithLocalizableError(SaleContentPriceWithDiscountMaxMessage.Instance);
			});

		RuleFor(x => x).NotEmpty().WithLocalizableError(SaleContentListNotEmptyMessage.Instance);
	}
}
