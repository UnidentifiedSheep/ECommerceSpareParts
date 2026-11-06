using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Exceptions;
using Main.Entities;
using Main.Entities.Exceptions;
using Main.Entities.Order;
using Main.Enums.Orders;
using MediatR;

namespace Main.Application.Orders.Commands.ChangeOrderStatus;

[Transactional, AutoSave]
public record ChangeOrderStatusCommand(
	Guid OrderId,
	Guid ActorId,
	OrderStatus DesiredStatus,
	uint RowVersion) : ICommand;

public class ChangeOrderStatusHandler(
	IRepository<Order, Guid> repository
	) : ICommandHandler<ChangeOrderStatusCommand>
{
	public async Task<Unit> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
	{
		var order = await repository.GetById(request.OrderId, cancellationToken)
			?? throw new OrderNotFoundException(request.OrderId);

		order.ValidateVersion(request.RowVersion);

		switch (request.DesiredStatus)
		{
			case OrderStatus.Confirmed:
				order.Confirm(request.ActorId);
				break;
			case OrderStatus.Completed:
				order.Complete();
				break;
			case OrderStatus.Cancelled:
				order.Cancel();
				break;
			default:
				throw new InvalidInputException(OrderChangeNotAllowedMessage.Instance);
		}

		return Unit.Value;
	}
}
