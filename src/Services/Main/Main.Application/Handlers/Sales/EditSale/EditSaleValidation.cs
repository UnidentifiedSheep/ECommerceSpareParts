using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.Sales.BaseValidators;
using Main.Entities;

namespace Main.Application.Handlers.Sales.EditSale;

public class EditSaleValidation : AbstractValidator<EditSaleCommand>
{
	public EditSaleValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.SaleId).NotEmpty().WithLocalizableError(SaleIdNotEmptyMessage.Instance);

		RuleFor(x => x.SaleDateTime).SetValidator(new RecordDateValidator(datePolicy));

		RuleFor(x => x.Content).SetValidator(new EditSaleContentsValidator());
	}
}
