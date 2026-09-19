using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.EditContent;

public class EditStorageContentValidation : AbstractValidator<EditStorageContentCommand>
{
	public EditStorageContentValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.EditedFields)
			.NotEmpty()
			.WithLocalizableError(StorageContentEditListNotEmptyMessage.Instance);

		RuleFor(x => x.EditedFields)
			.Must(x => x.Count < 100)
			.WithLocalizableError(StorageContentEditMaxCountMessage.Instance);

		RuleForEach(x => x.EditedFields.Values)
			.ChildRules(z =>
			{
				z
					.RuleFor(x => x.Model.BuyPrice.Value)
					.SetValidator(new PriceValidator())
					.When(x => x.Model.BuyPrice.IsSet);

				z
					.RuleFor(x => x.Model.Count.Value)
					.GreaterThanOrEqualTo(0)
					.When(x => x.Model.Count.IsSet)
					.WithLocalizableError(StorageContentCountMinZeroMessage.Instance);

				z
					.RuleFor(x => x.Model.PurchaseDatetime.Value)
					.SetValidator(new RecordDateValidator(datePolicy))
					.When(x => x.Model.PurchaseDatetime.IsSet);
			});
	}
}
