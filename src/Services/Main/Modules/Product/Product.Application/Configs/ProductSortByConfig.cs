using Application.Common.Extensions;
using Application.Common.Querying;
using Main.Entities.Producer;
using Main.Entities.Product;

namespace Main.Application.Configs;

public static class ProductSortByConfig
{
	public static void Configure()
	{
		QueryableSortBy
			.Value
			.MapDefault<Product, int>(x => x.Id)
			.Map<Product, int>("id", x => x.Id)
			.Map<Product, string>("sku", x => x.Sku.NormalizedValue)
			.Map<Product, string>("name", x => x.Name.Value)
			.Map<Product, string>("producerName", x => x.Producer.Name)
			.Map<Product, string>("indicator", x => x.Indicator!.Value!)
			.Map<Product, long>("popularity", x => x.Popularity)
			.Map<Product, long>("count", x => x.Stocks.Sum(stock => (long)stock.Quantity))
			.MapParameterized<Product, string, long>(
				"count",
				(product, storage) => product.Stocks
					.Where(stock => stock.StorageCode == storage)
					.Sum(stock => (long)stock.Quantity));

		QueryableSortBy
			.Value
			.MapDefault<Producer, int>(x => x.Id)
			.Map<Producer, int>("id", x => x.Id)
			.Map<Producer, string>("name", x => x.Name);

		QueryableSortBy
			.Value
			.MapDefault<Characteristic, int>(x => x.Id)
			.Map<Characteristic, int>("id", x => x.Id)
			.Map<Characteristic, string>("name", x => x.Name)
			.Map<Characteristic, string>("normalizedName", x => x.NormalizedName);

		QueryableSortBy
			.Value
			.MapDefault<ProductGroup, int>(x => x.Id)
			.Map<ProductGroup, int>("id", x => x.Id)
			.Map<ProductGroup, string>("name", x => x.Name)
			.Map<ProductGroup, string>("normalizedName", x => x.NormalizedName);
	}
}
