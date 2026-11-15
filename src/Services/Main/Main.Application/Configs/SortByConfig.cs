using Application.Common.Extensions;
using Application.Common.Querying;
using Main.Entities.Documents;
using Main.Entities.Order;
using Main.Entities.Organization;
using Main.Entities.Producer;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Orders;
using Main.Enums.Organization;

namespace Main.Application.Configs;

public static class SortByConfig
{
	public static void Configure()
	{
		ProductSortByConfig.Configure();

		QueryableSortBy.Value
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

		QueryableSortBy
			.Value
			.MapDefault<SaleContent, DateTime>(x => x.Sale.SaleDatetime, true)
			.Map<SaleContent, int>("saleContentId", x => x.Id)
			.Map<SaleContent, string>("storageCode", x => x.Sale.StorageCode)
			.Map<SaleContent, int>("quantity", x => x.Count)
			.Map<SaleContent, decimal>("originalPrice", x => x.OriginalPrice)
			.Map<SaleContent, decimal>("discountedPrice", x => x.DiscountedPrice)
			.Map<SaleContent, decimal>(
				"averageBuyPrice",
				x => x.Details.Sum(detail => detail.BuyPrice * detail.Count) / x.Count)
			.Map<SaleContent, DateTime>("saleDate", x => x.Sale.SaleDatetime)
			.MapParameterized<SaleContent, Guid, bool>(
				"organizationMatch",
				(content, organizationId) => content.Sale.OrganizationId == organizationId);

		QueryableSortBy.Value.ConfigureForJob();

		QueryableSortBy
			.Value
			.MapDefault<DocumentGenerationRequest, DateTime>(x => x.CreatedAt, true)
			.Map<DocumentGenerationRequest, Guid>("requestId", x => x.RequestId)
			.Map<DocumentGenerationRequest, Guid>("jobId", x => x.JobId)
			.Map<DocumentGenerationRequest, Guid?>("requesterId", x => x.RequesterId)
			.Map<DocumentGenerationRequest, string>("documentSystemName", x => x.DocumentSystemName)
			.Map<DocumentGenerationRequest, DateTime>("createdAt", x => x.CreatedAt)
			.Map<DocumentGenerationRequest, DateTime?>("generatedAt", x => x.GeneratedAt)
			.Map<DocumentGenerationRequest, DateTime?>("expiresAt", x => x.ExpiresAt);

		StorageSortByConfig.Configure();
	}
}
