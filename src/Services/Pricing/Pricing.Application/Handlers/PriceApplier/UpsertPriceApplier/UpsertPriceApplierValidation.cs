using Application.Common.Extensions;
using FluentValidation;
using Pricing.Entities;

namespace Pricing.Application.Handlers.PriceApplier.UpsertPriceApplier;

public class UpsertPriceApplierValidation : AbstractValidator<UpsertPriceApplierCommand>
{
	public UpsertPriceApplierValidation()
	{
		RuleFor(x => x.Name)
			.MaximumLength(128)
			.WithLocalizableError(PriceApplierNameMaxLengthMessage.Instance);

		RuleFor(x => x.States)
			.Must(states => states.Select(x => x.Usage).Distinct().Count() == states.Count)
			.WithLocalizableError(PriceApplierUsageDuplicateMessage.Instance);
	}
}
