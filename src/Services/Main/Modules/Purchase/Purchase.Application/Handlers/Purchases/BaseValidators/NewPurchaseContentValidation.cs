using Application.Common.Validators;
using FluentValidation;
using Main.Modules.Purchase.Application.Dtos.Purchase;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.BaseValidators;

public class NewPurchaseContentValidation : AbstractValidator<NewPurchaseContentDto>
{
	public NewPurchaseContentValidation()
	{
		RuleFor(x => x.Price).SetValidator(new PriceValidator());
		RuleFor(x => x.Count).SetValidator(new CountValidator());
	}
}
