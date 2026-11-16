using Application.Common.Querying;
using Main.Entities.Sale;

namespace Main.Application.Configs;

public static class SaleSortByConfig
{
	public static void Configure()
	{
		QueryableSortBy
			.Value
			.MapDefault<Sale, DateTime>(x => x.SaleDatetime, true)
			.Map<Sale, DateTime>("dateTime", x => x.SaleDatetime)
			.Map<Sale, decimal>("totalSum", x => x.Contents.Sum(content => content.TotalSum))
			.Map<Sale, Guid>("id", x => x.Id);

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
	}
}
