using Application.Common.Extensions;
using FluentValidation;
using Pricing.Entities;

namespace Pricing.Application.Commands.PriceApplier.DeletePriceApplier;

public class DeletePriceApplierValidation : AbstractValidator<DeletePriceApplierCommand>
{
	public DeletePriceApplierValidation()
	{
		RuleFor(x => x.SystemName)
			.NotEmpty()
			.WithLocalizableError(PriceApplierSystemNameRequiredMessage.Instance);
	}
}
