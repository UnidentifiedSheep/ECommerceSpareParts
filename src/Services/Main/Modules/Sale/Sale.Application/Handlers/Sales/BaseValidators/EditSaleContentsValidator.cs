using Application.Common.Extensions;
using FluentValidation;
using Main.Entities.Sale;
using Main.Modules.Sale.Application.Dtos.Sale;

namespace Main.Modules.Sale.Application.Handlers.Sales.BaseValidators;

public class EditSaleContentsValidator : AbstractValidator<IReadOnlyCollection<EditSaleContentDto>>
{
	public EditSaleContentsValidator()
	{
		RuleFor(x => x).NotEmpty().WithLocalizableError(SaleContentListNotEmptyMessage.Instance);

		RuleFor(x => x)
			.Must(list =>
			{
				var seen = new HashSet<int>();
				foreach (var item in list)
					if (item.Id.HasValue && !seen.Add(item.Id.Value))
						return false;

				return true;
			})
			.WithLocalizableError(SaleContentListDuplicateMessage.Instance);

		RuleForEach(x => x).SetValidator(new EditSaleContentValidator());
	}
}
