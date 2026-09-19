using Application.Common.Extensions;
using Application.Common.Services;
using Application.Common.Validators;
using FluentValidation;
using Main.Application.Handlers.Sales.BaseValidators;
using Main.Entities;

namespace Main.Application.Handlers.Sales.CreateSale;

public class CreateSaleValidation : AbstractValidator<CreateSaleCommand>
{
	public CreateSaleValidation(IOperationDatePolicy datePolicy)
	{
		RuleFor(x => x.SaleDateTime).SetValidator(new RecordDateValidator(datePolicy));

		RuleFor(x => x.Contents).SetValidator(new NewSaleContentValidator());

		RuleFor(x => x.PayedSum)
			.GreaterThanOrEqualTo(0)
			.When(x => x.PayedSum != null)
			.WithLocalizableError(SalePayedSumMinMessage.Instance)
			.PrecisionScale(
				18,
				2,
				true)
			.When(x => x.PayedSum != null)
			.WithLocalizableError(SalePayedSumPrecisionMessage.Instance);

		RuleFor(x => x.UserId).NotEmpty().WithLocalizableError(SaleBuyerIdNotEmptyMessage.Instance);

		RuleFor(x => x.OrganizationId)
			.NotEmpty()
			.WithLocalizableError(SaleOrganizationIdNotEmptyMessage.Instance);
	}
}
