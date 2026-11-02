using Application.Common.Abstractions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Main.Application.Interfaces.Persistence;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Exceptions;
using ProductEntity = Main.Entities.Product.Product;
using Main.Entities.Storage;

namespace Main.Application.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class UpdateProductStockHandler(
	IProductRepository productRepository,
	IRepository<ProductStock, (string, int)> stockRepository,
	IUnitOfWork unitOfWork) : BatchableDomainEventHandler<StorageContentCountUpdatedDomainEvent>
{
	public override async Task Handle(
		Batch<StorageContentCountUpdatedDomainEvent> notification,
		CancellationToken cancellationToken)
	{
		var deltas = notification.Items
			.GroupBy(item => (item.StorageCode, item.ProductId))
			.Select(group => new { group.Key, Delta = checked((int)group.Sum(item => (long)item.Delta)) })
			.Where(item => item.Delta != 0)
			.ToArray();

		if (deltas.Length == 0) return;

		var productIds = deltas.Select(item => item.Key.ProductId).Distinct().ToList();
		// Product locks also serialize creation of stock rows that do not exist yet.
		var products = await productRepository.FindByIdsAsync(
			productIds,
			Criteria<ProductEntity>.New().OrderByAsc(product => product.Id).Track().ForUpdate().Build(),
			cancellationToken);
		productIds.EnsureAllExists(products.Keys, notFound => new ProductNotFoundException(notFound));

		var storageCodes = deltas.Select(item => item.Key.StorageCode).Distinct().ToList();
		var stocks = (await stockRepository.ListAsync(
			Criteria<ProductStock>.New()
				.Where(stock => productIds.Contains(stock.ProductId) && storageCodes.Contains(stock.StorageCode))
				.Track().Build(), cancellationToken)).ToDictionary(stock => stock.GetId());

		var newStocks = new List<ProductStock>();
		foreach (var item in deltas)
			if (stocks.TryGetValue(item.Key, out var stock))
				stock.SetQuantity(checked(stock.Quantity + item.Delta));
			else
				newStocks.Add(ProductStock.Create(item.Key.StorageCode, item.Key.ProductId, item.Delta));

		if (newStocks.Count > 0)
			await unitOfWork.AddRangeAsync(newStocks, cancellationToken);

		foreach (var group in deltas.GroupBy(item => item.Key.ProductId))
			products[group.Key].IncreaseStock(checked((int)group.Sum(item => (long)item.Delta)));
	}
}
