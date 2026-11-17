using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities.Purchase;
using Main.Modules.Purchase.Application.Handlers.Purchases.BaseValidators;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.CreatePurchase;

public class CreatePurchaseValidation : AbstractValidator<CreatePurchaseCommand>
{
	public CreatePurchaseValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.PurchaseContent)
			.NotEmpty()
			.WithLocalizableError(PurchaseContentNotEmptyMessage.Instance);

		RuleFor(x => x.SupplierUserId)
			.NotEmpty()
			.WithLocalizableError(PurchaseSupplierIdNotEmptyMessage.Instance);

		RuleFor(x => x.SupplierOrganizationId)
			.NotEmpty()
			.WithLocalizableError(PurchaseSupplierOrganizationIdNotEmptyMessage.Instance);

		RuleForEach(x => x.PurchaseContent).SetValidator(new NewPurchaseContentValidation());

		RuleFor(x => x.PurchaseDate).SetValidator(new RecordDateValidator(datePolicy));

		RuleFor(x => x.PayedSum)
			.GreaterThanOrEqualTo(0)
			.When(x => x.PayedSum != null)
			.WithLocalizableError(PurchasePayedSumMinValueMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.When(x => x.PayedSum != null)
			.WithLocalizableError(PurchasePayedSumPrecisionMessage.Instance);
	}
}
