using Application.Common.Querying;
using Main.Entities.Order;
using Main.Enums.Orders;

namespace Main.Application.Configs;

public static class OrderSortByConfig
{
	public static void Configure()
		=> QueryableSortBy.Value
			.MapDefault<Order, DateTime>(order => order.CreatedAt, true)
			.Map<Order, Guid>("id", order => order.Id)
			.Map<Order, DateTime>("createdAt", order => order.CreatedAt)
			.Map<Order, DateTime>("updatedAt", order => order.UpdatedAt)
			.Map<Order, DateTime?>("confirmedAt", order => order.ConfirmedAt)
			.Map<Order, int>(
				"status",
				order => order.Status == OrderStatus.Pending ? (int)OrderStatus.Pending :
					order.Status == OrderStatus.Confirmed ? (int)OrderStatus.Confirmed :
					order.Status == OrderStatus.Completed ? (int)OrderStatus.Completed :
					(int)OrderStatus.Cancelled)
			.Map<Order, int>(
				"fulfillmentStatus",
				order =>
					order.FulfillmentStatus == OrderFulfillmentStatus.NotAvailable ?
						(int)OrderFulfillmentStatus.NotAvailable :
						order.FulfillmentStatus == OrderFulfillmentStatus.PartiallyAvailable ?
							(int)OrderFulfillmentStatus.PartiallyAvailable :
							(int)OrderFulfillmentStatus.FullyAvailable)
			.Map<Order, OrderSource>("source", order => order.Source);
}
