using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.Extensions;
using Exceptions;
using Main.Entities;
using Main.Application.Orders.Dtos;
using Main.Entities.Exceptions;
using Main.Entities.Order;

namespace Main.Application.Orders.Commands.UpsertOrder;

[AutoSave]
[Transactional(retryDelayMs: 10, retryCount: 2)]
public record UpsertOrderCommand(
	UpsertOrderDto Order) : ICommand<UpsertOrderResult>;

public record UpsertOrderResult(Guid OrderId);

public class UpsertOrderHandler(
	IRepository<Order, Guid> repository,
	IUnitOfWork unitOfWork
	) : ICommandHandler<UpsertOrderCommand, UpsertOrderResult>
{
	public async Task<UpsertOrderResult> Handle(
		UpsertOrderCommand request,
		CancellationToken cancellationToken)
	{
		var dto = request.Order;
		var order = await GetOrder(dto, cancellationToken);

		order.ChangeOrganization(dto.OrganizationId, dto.UserId);
		order.ChangeCurrency(dto.CurrencyId);

		ProcessItems(dto, order);

		return new UpsertOrderResult(order.Id);
	}

	private static void ProcessItems(UpsertOrderDto dto, Order order)
	{
		var existingItems = order.Items.Where(item => !item.IsDeleted).ToDictionary(x => x.Id);
		var requestedIds = dto.Items.Where(item => item.Id.HasValue).Select(item => item.Id!.Value).ToHashSet();
		var toRemove = existingItems.Values.Where(item => !requestedIds.Contains(item.Id)).ToArray();

		foreach (var upDto in dto.Items)
		{
			if (upDto.Id == null)
			{
				var item = order.AddItem(upDto.ProductId, upDto.Count);
				if (upDto.UnitPrice.HasValue)
					order.ChangeItemPrice(item, upDto.UnitPrice);
				continue;
			}

			if (!existingItems.TryGetValue(upDto.Id.Value, out var existing))
				throw new InvalidInputException(OrderItemNotFoundMessage.Instance);

			if (existing.UnitPrice != upDto.UnitPrice)
				order.ChangeItemPrice(existing, upDto.UnitPrice);

			order.ChangeItemCount(existing, upDto.Count);
			order.ChangeItemProduct(existing, upDto.ProductId);
		}

		foreach (var item in toRemove)
			order.RemoveItem(item);
	}

	private async Task<Order> GetOrder(UpsertOrderDto dto, CancellationToken ct)
	{
		var order = dto.Id.HasValue
			? await repository.FirstOrDefaultAsync(
				Criteria<Order>.New()
					.Where(x => x.Id == dto.Id.Value)
					.Track()
					.Include(x => x.Items)
					.Build(),
				ct) ?? throw new OrderNotFoundException(dto.Id.Value)
			: Order.CreateManual(
				dto.OrganizationId,
				dto.UserId,
				dto.CurrencyId);

		if (dto.Id.HasValue)
			order.ValidateVersion(dto.RowVersion ??
				throw new InvalidInputException(OrderRowVersionRequiredMessage.Instance));
		else
			await unitOfWork.AddAsync(order, ct);

		return order;
	}
}
