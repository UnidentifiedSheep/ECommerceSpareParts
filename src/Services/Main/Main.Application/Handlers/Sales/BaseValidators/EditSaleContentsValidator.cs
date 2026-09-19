using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Dtos.Sale;
using Main.Entities;

namespace Main.Application.Handlers.Sales.BaseValidators;

public class EditSaleContentsValidator : AbstractValidator<IEnumerable<EditSaleContentDto>>
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
