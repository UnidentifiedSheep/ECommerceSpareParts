using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Localization.Domain.Extensions;
using Main.Application.Handlers.Purchases.BaseValidators;

namespace Main.Application.Handlers.Purchases.EditPurchase;

public class EditPurchaseValidation : AbstractValidator<EditPurchaseCommand>
{
	public EditPurchaseValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.PurchaseId).NotEmpty().WithLocalizationKey("purchase.id.not.empty");

		RuleFor(x => x.Content).NotEmpty().WithLocalizationKey("purchase.content.not.empty");

		RuleFor(x => x.PurchaseDateTime).SetValidator(new RecordDateValidator(datePolicy));

		RuleFor(x => x.Content).SetValidator(new EditPurchaseDtoValidation());

		RuleFor(x => x.StorageFrom)
			.Must(x => x != null)
			.When(x => x.WithLogistics)
			.WithLocalizationKey("purchase.storage.from.required.when.logistics");
	}
}
