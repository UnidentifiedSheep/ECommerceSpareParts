using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Application.Common.Extensions;
using Main.Entities;
using Main.Application.Handlers.BaseValidators;

namespace Main.Application.Handlers.StorageContents.AddContent;

public class AddContentValidation : AbstractValidator<AddContentCommand>
{
	public AddContentValidation(IOperationDatePolicy datePolicy)
	{
		RuleForEach(x => x.StorageContent)
			.ChildRules(content =>
			{
				content.RuleFor(x => x.BuyPrice).SetValidator(new PriceValidator());

				content.RuleFor(x => x.Count).SetValidator(new CountValidator());

				content.RuleFor(x => x.PurchaseDate).SetValidator(new RecordDateValidator(datePolicy));
			});

		RuleFor(x => x.StorageContent).NotEmpty().WithLocalizableError(StorageContentListNotEmptyMessage.Instance);

		RuleFor(x => x.StorageCode).NotEmpty().WithLocalizableError(StorageNameNotEmptyMessage.Instance);
	}
}
