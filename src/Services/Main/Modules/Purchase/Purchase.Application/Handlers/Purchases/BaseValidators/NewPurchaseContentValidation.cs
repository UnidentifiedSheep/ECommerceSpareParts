using FluentValidation;
using Main.Entities.Purchase;
using Main.Application.Dtos.Purchase;
using Application.Common.Validators;

namespace Main.Application.Handlers.Purchases.BaseValidators;

public class NewPurchaseContentValidation : AbstractValidator<NewPurchaseContentDto>
{
	public NewPurchaseContentValidation()
	{
		RuleFor(x => x.Price).SetValidator(new PriceValidator());
		RuleFor(x => x.Count).SetValidator(new CountValidator());
	}
}
