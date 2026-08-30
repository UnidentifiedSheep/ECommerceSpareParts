using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Settings;
using Enums;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Static;
using Pricing.Entities.Settings;

namespace Pricing.Application.Services;

public class SupplierOfferRequestMarkerService(ISettingsService settingsService, ICache cache)
	: ISupplierOfferRequestMarkerService
{
	public async Task<bool> HasAnyMarkerAsync(
		Supplier supplier,
		int productId,
		string storageCode,
		CancellationToken token) => (await cache.KeyExistsAsync(
		[
			CacheKeys.Offer.Failed.Key(
				supplier,
				productId,
				storageCode),
			CacheKeys.Offer.Ok.Key(
				supplier,
				productId,
				storageCode)
		],
		token)).Any(x => x.Value);

	public async Task MarkAsOkAsync(
		Supplier supplier,
		int productId,
		string storageCode,
		CancellationToken token) => await cache.SetAsync(
		CacheKeys.Offer.Ok.Key(
			supplier,
			productId,
			storageCode),
		true,
		CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));

	public async Task MarkAsOkAsync(
		IEnumerable<int> productId,
		Supplier supplier,
		string storageCode,
		CancellationToken token)
	{
		var keys = productId
			.Select(x => (CacheKeys.Offer.Ok.Key(
				supplier,
				x,
				storageCode), true))
			.ToArray();

		if (keys.Length == 0)
			return;

		await cache.SetAsync(
			keys,
			CacheKeys.Offer.Ok.Ttl((await settingsService.GetOrDefault<PricingSetting>(token)).Data));
	}

	public async Task MarkAsFailedAsync(
		Supplier supplier,
		int productId,
		string storageCode) => await cache.SetAsync(
		CacheKeys.Offer.Failed.Key(
			supplier,
			productId,
			storageCode),
		true,
		CacheKeys.Offer.Failed.Ttl);
}
