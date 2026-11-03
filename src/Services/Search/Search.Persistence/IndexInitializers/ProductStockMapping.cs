using OpenSearch.Client;
using Search.Entities;

namespace Search.Persistence.IndexInitializers;

internal static class ProductStockMapping
{
	public static PropertiesDescriptor<Product> StockFields(this PropertiesDescriptor<Product> properties) => properties
		.Number(number => number.Name(product => product.Stock).Type(NumberType.Long))
		.Number(number => number.Name(product => product.StockSnapshotVersion).Type(NumberType.Long).Index(false).DocValues(false))
		.Object<Dictionary<string, int>>(obj => obj.Name(product => product.Stocks).Dynamic(true))
		.Object<Dictionary<string, long>>(obj => obj.Name(product => product.StockVersions).Enabled(false));
}
