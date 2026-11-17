using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Stock;
using Main.Modules.Product.Abstractions;
using Main.Modules.Product.Application.Interfaces.Persistence;

namespace Main.Modules.Product.Application.Services;

public sealed class ProductStockWriter(
	IProductRepository productRepository,
	IRepository<ProductStock, (string, int)> stockRepository,
	IUnitOfWork unitOfWork) : IProductStockWriter
{
	public async Task ApplyAsync(
		IReadOnlyCollection<ProductStockDelta> deltas,
		CancellationToken cancellationToken = default)
	{
		if (deltas.Count == 0) return;

		var keyedDeltas = deltas.ToDictionary(item => (item.StorageCode, item.ProductId), item => item.Delta);
		var productIds = deltas.Select(item => item.ProductId).Distinct().ToList();
		var storageCodes = deltas.Select(item => item.StorageCode).Distinct().ToList();
		var existingKeys = (await stockRepository.ListAsync(
				Criteria<ProductStock>.New()
					.Where(stock
						=> productIds.Contains(stock.ProductId) && storageCodes.Contains(stock.StorageCode))
					.Build(),
				cancellationToken)).Select(stock => stock.GetId())
			.ToHashSet();
		var missingProductIds = keyedDeltas.Keys
			.Where(key => !existingKeys.Contains(key))
			.Select(key => key.ProductId)
			.Distinct()
			.ToList();

		if (missingProductIds.Count > 0)
		{
			var products = await productRepository.FindByIdsAsync(
				missingProductIds,
				Criteria<Entities.Product.Product.Product>.New()
					.OrderByAsc(product => product.Id)
					.ForUpdate()
					.Build(),
				cancellationToken);
			missingProductIds.EnsureAllExists(
				products.Keys,
				notFound => new ProductNotFoundException(notFound));
		}

		var stocks = (await stockRepository.ListAsync(
			new Criteria<ProductStock>
			{
				Wheres =
				[
					stock => productIds.Contains(stock.ProductId) && storageCodes.Contains(stock.StorageCode)
				],
				OrderBy = query => query.OrderBy(stock => stock.StorageCode).ThenBy(stock => stock.ProductId),
				Track = true,
				ForUpdate = true
			},
			cancellationToken)).ToDictionary(stock => stock.GetId());

		var newStocks = new List<ProductStock>();
		foreach (var (key, delta) in keyedDeltas)
			if (stocks.TryGetValue(key, out var stock))
				stock.SetQuantity(checked(stock.Quantity + delta));
			else
				newStocks.Add(
					ProductStock.Create(
						key.StorageCode,
						key.ProductId,
						delta));

		if (newStocks.Count > 0)
			await unitOfWork.AddRangeAsync(newStocks, cancellationToken);
	}
}
