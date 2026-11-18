using Application.Common.Querying;
using Main.Entities.Sale.Sale;

namespace Main.Modules.Sale.Application.Configs;

public static class SaleSortByConfig
{
	public static void Configure()
	{
		QueryableSortBy
			.Value
			.MapDefault<Entities.Sale.Sale.Sale, DateTime>(x => x.SaleDatetime, true)
			.Map<Entities.Sale.Sale.Sale, DateTime>("dateTime", x => x.SaleDatetime)
			.Map<Entities.Sale.Sale.Sale, decimal>(
				"totalSum",
				x => x.Contents.Sum(content => content.TotalSum))
			.Map<Entities.Sale.Sale.Sale, Guid>("id", x => x.Id);

		QueryableSortBy
			.Value
			.MapDefault<SaleContent, DateTime>(x => x.Sale.SaleDatetime, true)
			.Map<SaleContent, int>("saleContentId", x => x.Id)
			.Map<SaleContent, string>("storageCode", x => x.Sale.StorageCode)
			.Map<SaleContent, int>("quantity", x => x.Count)
			.Map<SaleContent, decimal>("originalPrice", x => x.OriginalPrice)
			.Map<SaleContent, decimal>("discountedPrice", x => x.DiscountedPrice)
			.Map<SaleContent, DateTime>("saleDate", x => x.Sale.SaleDatetime)
			.MapParameterized<SaleContent, Guid, bool>(
				"organizationMatch",
				(content, organizationId) => content.Sale.OrganizationId == organizationId);
	}
}
