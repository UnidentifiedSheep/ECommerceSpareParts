using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Pricing.Entities.Enums;

namespace Pricing.Entities.Offers;

public class PriceOfferDailyStat : Entity<PriceOfferDailyStat, PriceOfferDailyStatKey>,
	ILinqEntity<PriceOfferDailyStat, PriceOfferDailyStatKey>
{
	private PriceOfferDailyStat()
	{
	}

	public DateOnly Day { get; private set; }

	public int ProductId { get; private set; }

	public PriceOfferSource Source { get; private set; }

	public string ForStorage { get; private set; } = string.Empty;

	public int CurrencyId { get; private set; }

	public decimal PurchasePrice { get; private set; }

	public int OffersCount { get; private set; }

	public DateTime FirstSeenAt { get; private set; }

	public DateTime LastSeenAt { get; private set; }

	public static Expression<Func<PriceOfferDailyStat, PriceOfferDailyStatKey>> GetKeySelector()
		=> x => new PriceOfferDailyStatKey(
			x.ProductId,
			x.Source,
			x.ForStorage,
			x.Day,
			x.CurrencyId,
			x.PurchasePrice);

	public static Expression<Func<PriceOfferDailyStat, bool>> GetEqualityExpression(PriceOfferDailyStatKey key)
		=> x => x.ProductId == key.ProductId &&
			x.Source == key.Source &&
			x.ForStorage == key.ForStorage &&
			x.Day == key.Day &&
			x.CurrencyId == key.CurrencyId &&
			x.PurchasePrice == key.PurchasePrice;

	public static PriceOfferDailyStat Create(
		int productId,
		PriceOfferSource source,
		string forStorage,
		int currencyId,
		decimal purchasePrice,
		int offersCount,
		DateTime seenAt)
		=> new()
		{
			Day = DateOnly.FromDateTime(seenAt),
			ProductId = productId,
			Source = source,
			ForStorage = forStorage,
			CurrencyId = currencyId,
			PurchasePrice = purchasePrice,
			OffersCount = offersCount,
			FirstSeenAt = seenAt,
			LastSeenAt = seenAt
		};

	public static PriceOfferDailyStat Merge(PriceOfferDailyStat left, PriceOfferDailyStat right)
		=> new()
		{
			Day = left.Day,
			ProductId = left.ProductId,
			Source = left.Source,
			ForStorage = left.ForStorage,
			CurrencyId = left.CurrencyId,
			PurchasePrice = left.PurchasePrice,
			OffersCount = left.OffersCount + right.OffersCount,
			FirstSeenAt = left.FirstSeenAt < right.FirstSeenAt ? left.FirstSeenAt : right.FirstSeenAt,
			LastSeenAt = left.LastSeenAt > right.LastSeenAt ? left.LastSeenAt : right.LastSeenAt
		};

	public override PriceOfferDailyStatKey GetId()
		=> new(
			ProductId,
			Source,
			ForStorage,
			Day,
			CurrencyId,
			PurchasePrice);
}

public readonly record struct PriceOfferDailyStatKey(
	int ProductId,
	PriceOfferSource Source,
	string ForStorage,
	DateOnly Day,
	int CurrencyId,
	decimal PurchasePrice) : ICompositeKey
{
	public object[] ToArray() => [ProductId, Source, ForStorage, Day, CurrencyId, PurchasePrice];
}
