using Main.Entities.Product;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class ProductEanTestContext(UnifiedDbContext context, ProductTestContext productTestContext)
	: TestContextBase<UnifiedDbContext>(context), IDependentTestContext
{
	public ProductTestContext ProductTestContext => productTestContext;

	public IReadOnlyCollection<ProductEan> ProductEans { get; private set; } = null!;

	public static Type[] DependsOn { get; } = [typeof(ProductTestContext)];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var builders = productTestContext.Products
			.Take(3)
			.Select(product => new ProductEanBuilder(Faker).WithProductId(product.Id));

		ProductEans = await builders.BuildManyCombinedAndAddToDb(DbContext, 3);
	}
}
