using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.Purchases.BaseValidators;
using Main.Entities;

namespace Main.Application.Handlers.Purchases.EditPurchase;

public class EditPurchaseValidation : AbstractValidator<EditPurchaseCommand>
{
	public EditPurchaseValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.PurchaseId).NotEmpty().WithLocalizableError(PurchaseIdNotEmptyMessage.Instance);

		RuleFor(x => x.Content).NotEmpty().WithLocalizableError(PurchaseContentNotEmptyMessage.Instance);

		RuleFor(x => x.PurchaseDateTime).SetValidator(new RecordDateValidator(datePolicy));

		RuleFor(x => x.Content).SetValidator(new EditPurchaseDtoValidation());

		RuleFor(x => x.StorageFrom)
			.Must(x => x != null)
			.When(x => x.WithLogistics)
			.WithLocalizableError(PurchaseStorageFromRequiredWhenLogisticsMessage.Instance);
	}
}
