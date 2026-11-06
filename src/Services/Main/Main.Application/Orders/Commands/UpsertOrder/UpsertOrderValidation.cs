using Application.Common.Extensions;
using FluentValidation;
using Main.Application.Handlers.BaseValidators;
using Main.Entities;

namespace Main.Application.Orders.Commands.UpsertOrder;

public class UpsertOrderValidation : AbstractValidator<UpsertOrderCommand>
{
	public UpsertOrderValidation()
	{
		RuleFor(x => x.Order).NotNull().WithLocalizableError(OrderInputInvalidMessage.Instance);
		When(x => x.Order != null, () =>
		{
			RuleFor(x => x.Order.Id).NotEqual(Guid.Empty).WithLocalizableError(OrderInputInvalidMessage.Instance);
			RuleFor(x => x.Order.OrganizationId).NotEmpty().WithLocalizableError(OrderInputInvalidMessage.Instance);
			RuleFor(x => x.Order.UserId).NotEqual(Guid.Empty).WithLocalizableError(OrderInputInvalidMessage.Instance);
			RuleFor(x => x.Order.CurrencyId).GreaterThan(0).WithLocalizableError(OrderInputInvalidMessage.Instance);
			RuleFor(x => x.Order.RowVersion).NotNull().WithLocalizableError(OrderRowVersionRequiredMessage.Instance)
				.When(x => x.Order.Id.HasValue);
			RuleFor(x => x.Order.Items).NotEmpty().WithLocalizableError(OrderItemsInvalidMessage.Instance);
			When(x => x.Order.Items != null, () =>
			{
				RuleFor(x => x.Order.Items)
					.Must(items =>
					{
						var ids = items.Where(item => item?.Id != null).Select(item => item.Id!.Value).ToArray();
						return ids.Distinct().Count() == ids.Length;
					})
					.WithLocalizableError(OrderItemsInvalidMessage.Instance);
				RuleForEach(x => x.Order.Items).NotNull().WithLocalizableError(OrderItemInvalidMessage.Instance);
				RuleForEach(x => x.Order.Items).ChildRules(item =>
				{
					item.RuleFor(x => x.Count).GreaterThan(0).WithLocalizableError(OrderItemInvalidMessage.Instance);
					item.RuleFor(x => x.ProductId).GreaterThan(0).WithLocalizableError(OrderItemInvalidMessage.Instance);
					item.RuleFor(x => x.Id).GreaterThan(0).WithLocalizableError(OrderItemInvalidMessage.Instance);
					item.RuleFor(x => x.UnitPrice!.Value).SetValidator(new PriceValidator()).When(x => x.UnitPrice.HasValue);
				});
				RuleFor(x => x.Order.Items).Must(items => items.All(item => item == null || item.Id == null))
					.WithLocalizableError(OrderItemInvalidMessage.Instance).When(x => !x.Order.Id.HasValue);
			});
		});
	}
}
