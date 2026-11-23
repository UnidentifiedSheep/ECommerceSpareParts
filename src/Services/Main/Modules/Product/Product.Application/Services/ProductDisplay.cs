using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductDisplay(IReadRepository<Entities.Product.Product.Product, int> products)
	: IDisplay<int, ProductDisplayInfo>
{
	public async Task<IReadOnlyDictionary<int, ProductDisplayInfo>> GetAsync(
		IReadOnlyCollection<int> ids,
		CancellationToken cancellationToken = default)
		=> (await products.Query
				.Where(product => ids.Contains(product.Id))
				.Select(product => new
				{
					product.Id, Name = product.Name.Value, Sku = product.Sku.Value
				})
				.ToListAsync(cancellationToken))
			.ToDictionary(item => item.Id, item => new ProductDisplayInfo(item.Name, item.Sku));
}
