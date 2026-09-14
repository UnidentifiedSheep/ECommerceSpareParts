using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class EditSaleContentValidator : AbstractValidator<EditSaleContentDto>
{
	public EditSaleContentValidator()
	{
		RuleFor(x => x.Count).GreaterThan(0).WithLocalizableError(SaleContentCountMinMessage.Instance);

		RuleFor(x => x.Comment).MaximumLength(256).WithLocalizableError(SaleContentCommentMaxMessage.Instance);

		RuleFor(x => x.Price).SetValidator(new PriceValidator());

		RuleFor(x => x.PriceWithDiscount)
			.SetValidator(new PriceValidator())
			.LessThanOrEqualTo(x => x.Price)
			.WithLocalizableError(SaleContentPriceWithDiscountMaxMessage.Instance);
	}
}
