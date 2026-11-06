using Application.Common.Extensions;
using FluentValidation;
using Main.Entities;
using Main.Enums.Orders;

namespace Main.Application.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusValidation : AbstractValidator<ChangeOrderStatusCommand>
{
	public ChangeOrderStatusValidation()
	{
		RuleFor(x => x.OrderId).NotEmpty().WithLocalizableError(OrderInputInvalidMessage.Instance);
		RuleFor(x => x.DesiredStatus)
			.Must(status => status is OrderStatus.Confirmed or OrderStatus.Completed or OrderStatus.Cancelled)
			.WithLocalizableError(OrderChangeNotAllowedMessage.Instance);
	}
}
