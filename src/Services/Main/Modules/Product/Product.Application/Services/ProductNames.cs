using Application.Common.Interfaces.Repositories;
using Main.Modules.Product.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductNames(IReadRepository<Entities.Product.Product.Product, int> products) : IProductNames
{
	public async Task<IReadOnlyDictionary<int, ProductDisplayName>> GetAsync(
		IReadOnlyCollection<int> productIds,
		CancellationToken cancellationToken = default)
		=> (await products.Query
				.Where(product => productIds.Contains(product.Id))
				.Select(product => new
				{
					product.Id, Name = product.Name.Value, Sku = product.Sku.Value
				})
				.ToListAsync(cancellationToken))
			.ToDictionary(item => item.Id, item => new ProductDisplayName(item.Name, item.Sku));
}
