using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Main.Entities;

namespace Main.Application.Handlers.StorageContents.RestoreContent;

public class RestoreContentValidation : AbstractValidator<RestoreContentCommand>
{
	public RestoreContentValidation()
	{
		RuleForEach(z => z.ContentDetails)
			.ChildRules(z =>
			{
				z.RuleFor(x => x.Count).SetValidator(new CountValidator());

				z.RuleFor(x => x.BuyPrice).SetValidator(new PriceValidator());
			});

		RuleFor(x => x.ContentDetails)
			.NotEmpty()
			.WithLocalizableError(StorageContentRestoreListNotEmptyMessage.Instance);
	}
}
