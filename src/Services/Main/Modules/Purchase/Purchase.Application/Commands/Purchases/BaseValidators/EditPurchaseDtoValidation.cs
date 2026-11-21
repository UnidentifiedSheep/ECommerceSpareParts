using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities.Purchase;
using Main.Modules.Purchase.Application.Dtos.Purchase;

namespace Main.Modules.Purchase.Application.Commands.Purchases.BaseValidators;

public class EditPurchaseDtoValidation : AbstractValidator<IReadOnlyCollection<EditPurchaseDto>>
{
	public EditPurchaseDtoValidation()
	{
		RuleForEach(z => z)
			.ChildRules(z =>
			{
				z.RuleFor(x => x.Count).SetValidator(new CountValidator());

				z.RuleFor(x => x.Price).SetValidator(new PriceValidator());
			});

		RuleFor(z => z)
			.Must(z =>
			{
				var ids = z.Where(x => x.Id != null).Select(x => x.Id!.Value).ToList();
				var idsSet = ids.ToHashSet();
				return ids.Count == idsSet.Count;
			})
			.WithLocalizableError(PurchaseContentDuplicateIdsNotAllowedMessage.Instance);
	}
}
