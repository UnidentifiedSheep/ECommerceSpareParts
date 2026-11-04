using Application.Common;
using Application.Common.Extensions;
using Application.Common.Validators;
using FluentValidation;
using Main.Entities;

namespace Main.Application.Handlers.Orders.GetOrders;

public sealed class GetOrdersValidation : AbstractValidator<GetOrdersQuery>
{
	public GetOrdersValidation()
	{
		RuleFor(query => query.Pagination)
			.SetValidator(new PaginationValidator());

		RuleFor(query => query.DateRange)
			.Must(range => !range.Min.HasValue || !range.Max.HasValue || range.Min.Value <= range.Max.Value)
			.WithLocalizableError(OrderDateRangeInvalidMessage.Instance);

		RuleFor(query => query.SearchTerm)
			.MaximumLength(200)
			.WithLocalizableError(SearchStringMaxLengthMessage.Create(200));

		RuleForEach(query => query.Statuses)
			.IsInEnum()
			.WithLocalizableError(OrderFilterInvalidMessage.Instance);

		RuleForEach(query => query.FulfillmentStatuses)
			.IsInEnum()
			.WithLocalizableError(OrderFilterInvalidMessage.Instance);

		RuleForEach(query => query.Sources)
			.IsInEnum()
			.WithLocalizableError(OrderFilterInvalidMessage.Instance);
	}
}
