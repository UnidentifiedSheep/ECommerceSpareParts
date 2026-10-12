using Main.Entities.Product;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Interfaces;

namespace Tests.TestContexts;

public class ProductEanTestContext(DContext context, ProductTestContext productTestContext)
	: TestContextBase<DContext>(context), IDependentTestContext
{
	public ProductTestContext ProductTestContext => productTestContext;

	public IReadOnlyCollection<ProductEan> ProductEans { get; private set; } = null!;

	public static Type[] DependsOn { get; } = [typeof(ProductTestContext)];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var builders = productTestContext.Products.Take(3)
			.Select(product => new ProductEanBuilder(Faker).WithProductId(product.Id));

		ProductEans = await builders.BuildManyCombinedAndAddToDb(DbContext, 3);
	}
}
