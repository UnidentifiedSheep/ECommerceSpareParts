using Application.Common.Querying;
using Main.Entities.Order.Enums.Orders;

namespace Main.Modules.Order.Application.Configs;

public static class OrderSortByConfig
{
	public static void Configure()
		=> QueryableSortBy.Value
			.MapDefault<Entities.Order.Order.Order, DateTime>(order => order.CreatedAt, true)
			.Map<Entities.Order.Order.Order, Guid>("id", order => order.Id)
			.Map<Entities.Order.Order.Order, DateTime>("createdAt", order => order.CreatedAt)
			.Map<Entities.Order.Order.Order, DateTime>("updatedAt", order => order.UpdatedAt)
			.Map<Entities.Order.Order.Order, DateTime?>("confirmedAt", order => order.ConfirmedAt)
			.Map<Entities.Order.Order.Order, int>(
				"status",
				order => order.Status == OrderStatus.Pending ? (int)OrderStatus.Pending :
					order.Status == OrderStatus.Confirmed ? (int)OrderStatus.Confirmed :
					order.Status == OrderStatus.Completed ? (int)OrderStatus.Completed :
					(int)OrderStatus.Cancelled)
			.Map<Entities.Order.Order.Order, int>(
				"fulfillmentStatus",
				order =>
					order.FulfillmentStatus == OrderFulfillmentStatus.NotAvailable ?
						(int)OrderFulfillmentStatus.NotAvailable :
						order.FulfillmentStatus == OrderFulfillmentStatus.PartiallyAvailable ?
							(int)OrderFulfillmentStatus.PartiallyAvailable :
							(int)OrderFulfillmentStatus.FullyAvailable)
			.Map<Entities.Order.Order.Order, OrderSource>("source", order => order.Source);
}
