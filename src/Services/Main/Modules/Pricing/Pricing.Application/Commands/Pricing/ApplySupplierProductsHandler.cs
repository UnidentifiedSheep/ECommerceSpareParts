using Application.Common.Interfaces.Cqrs;
using Attributes;
using Enums;
using Integrations.Supplier.Models;
using Main.Modules.Product.Abstractions;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities.Offers;

namespace Pricing.Application.Commands.Pricing;

[Transactional]
public record ApplySupplierProductsCommand(
	DateTime DataExtractionTime,
	Supplier Supplier,
	string StorageCode,
	IReadOnlyList<SupplierProduct> Products) : ICommand<ApplySupplierProductsResult>;

public record ApplySupplierProductsResult(IReadOnlyList<PriceOffer> CreatedOffers);

public class ApplySupplierProductsHandler(IOfferRefreshService refreshService,
	IProductReferenceResolver referenceResolver)
	: ICommandHandler<ApplySupplierProductsCommand, ApplySupplierProductsResult>
{
	public async Task<ApplySupplierProductsResult> Handle(
		ApplySupplierProductsCommand request,
		CancellationToken cancellationToken)
	{
		var dict = new Dictionary<ProductKey, List<SupplierPosition>>();
		var queue = new Queue<SupplierProduct>(request.Products);

		if (queue.Count == 0)
			return new ApplySupplierProductsResult([]);

		while (queue.TryDequeue(out var product))
		{
			var key = new ProductKey(product.Brand, product.Number);

			if (!dict.TryAdd(key, product.Positions.ToList()))
				continue;

			foreach (var analogue in product.Analogues)
				queue.Enqueue(analogue);
		}

		var productIds = await ResolveProductIdsAsync(
			request.Supplier,
			dict,
			cancellationToken);

		var refreshDict = dict
			.Select(x => (
				ProductId: productIds.TryGetValue(x.Key, out var productId) ? productId : (int?)null,
				Positions: (IReadOnlyList<SupplierPosition>)x.Value.ToList()))
			.Where(x => x.ProductId != null)
			.GroupBy(x => x.ProductId!.Value)
			.ToDictionary(
				group => group.Key,
				IReadOnlyList<SupplierPosition> (group) => group
					.SelectMany(x => x.Positions)
					.DistinctBy(position => position.Id)
					.ToArray());

		var offers = await refreshService.RefreshOffersAsync(
			request.DataExtractionTime,
			request.StorageCode,
			request.Supplier,
			refreshDict,
			cancellationToken);

		return new ApplySupplierProductsResult(offers);
	}

	private async Task<Dictionary<ProductKey, int>> ResolveProductIdsAsync(
		Supplier supplier,
		Dictionary<ProductKey, List<SupplierPosition>> dict,
		CancellationToken cancellationToken)
	{
		var references = dict.Keys
			.Select(key => new ProductReference(
				key.Number,
				key.Brand,
				supplier))
			.ToArray();
		var resolved = await referenceResolver.ResolveAsync(references, cancellationToken);

		return resolved
			.Where(product => product.Supplier == supplier && product.ProductId.HasValue)
			.ToDictionary(
				product => new ProductKey(product.ProducerName, product.Sku),
				product => product.ProductId!.Value);
	}

	private readonly record struct ProductKey(string Brand, string Number);
}
