using Main.Application.Interfaces.Persistence;
using Main.Entities.Order;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Enums.Products;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using ProductEntity = Main.Entities.Product.Product;

namespace Main.Persistence.Repositories.Product;

public sealed class ProductDeletionBlockerProvider(UnifiedDbContext context) : IProductDeletionBlockerProvider
{
	public Task<ProductDeletionBlocker?> GetAsync(
		int productId,
		CancellationToken cancellationToken = default)
		=> context.Set<ProductEntity>()
			.AsNoTracking()
			.Where(product => product.Id == productId)
			.Select(product
				=> context.Set<ProductStock>()
					.Any(stock => stock.ProductId == product.Id && stock.Quantity > 0) ?
					(ProductDeletionBlocker?)ProductDeletionBlocker.Stock :
					context.Set<StorageContent>().Any(item => item.ProductId == productId) ?
						ProductDeletionBlocker.StorageContent :
						context.Set<SaleContent>().Any(item => item.ProductId == productId) ?
							ProductDeletionBlocker.Sale :
							context.Set<PurchaseContent>().Any(item => item.ProductId == productId) ?
								ProductDeletionBlocker.Purchase :
								context.Set<OrderItem>().Any(item => item.ProductId == productId) ?
									ProductDeletionBlocker.Order :
									context.Set<ProductContent>()
										.Any(item => item.ChildProductId == productId) ?
										ProductDeletionBlocker.Composition : null)
			.SingleAsync(cancellationToken);
}
