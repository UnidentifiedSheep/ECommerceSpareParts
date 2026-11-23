using Pricing.Application.Interfaces.Persistence;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;

namespace Pricing.Application.Services;

public class PriceOfferHistoryWriter(IPriceOfferHistoryRepository repository) : IPriceOfferHistoryWriter
{
	public async Task TrackAsync(
		string storageCode,
		PriceOfferSource source,
		IReadOnlyCollection<int> productIds,
		IReadOnlyCollection<PriceOffer> offers,
		DateTime seenAt,
		CancellationToken cancellationToken = default)
	{
		if (productIds.Count == 0)
			return;

		var open = await repository.GetOpenAsync(
			storageCode,
			source,
			productIds,
			cancellationToken);

		var openByProduct = open
			.GroupBy(x => x.ProductId)
			.ToDictionary(x => x.Key, x => x.ToList());

		var incoming = offers
			.Where(x => x.Source == source && x.OfferForStorage == storageCode)
			.Select(x => PriceOfferHistory.Open(x, seenAt))
			.DistinctBy(x => (x.ProductId, x.CurrencyId, x.PurchasePrice, x.DeliveryTime, x.DeliveryProbability))
			.ToList();

		var seen = new List<long>();
		var created = new List<PriceOfferHistory>();

		foreach (var history in incoming)
		{
			var candidates = openByProduct.GetValueOrDefault(history.ProductId);
			var match = candidates?.FirstOrDefault(x => x.SameValuesAs(history));

			if (match is null)
			{
				created.Add(history);
				continue;
			}

			candidates!.Remove(match);
			seen.Add(match.Id);
		}

		var closed = openByProduct.Values.SelectMany(x => x).Select(x => x.Id).ToList();

		await repository.CloseAsync(closed, cancellationToken);
		await repository.MarkSeenAsync(seen, seenAt, cancellationToken);
		await repository.AddAsync(created, cancellationToken);
	}
}
