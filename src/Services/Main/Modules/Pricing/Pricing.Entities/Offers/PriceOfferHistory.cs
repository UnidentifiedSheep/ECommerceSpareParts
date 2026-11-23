using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Pricing.Entities.Enums;

namespace Pricing.Entities.Offers;

public class PriceOfferHistory : Entity<PriceOfferHistory, long>, ILinqEntity<PriceOfferHistory, long>
{
	private PriceOfferHistory()
	{
	}

	public long Id { get; private set; }

	public int ProductId { get; private set; }

	public PriceOfferSource Source { get; private set; }

	public string ForStorage { get; private set; } = string.Empty;

	public int CurrencyId { get; private set; }

	public decimal PurchasePrice { get; private set; }

	public TimeSpan DeliveryTime { get; private set; }

	public int DeliveryProbability { get; private set; }

	public DateTime FirstSeenAt { get; private set; }

	public DateTime LastSeenAt { get; private set; }

	public bool IsOpen { get; private set; }

	public static Expression<Func<PriceOfferHistory, long>> GetKeySelector() => x => x.Id;

	public static Expression<Func<PriceOfferHistory, bool>> GetEqualityExpression(long key) => x => x.Id == key;

	public static PriceOfferHistory Open(PriceOffer offer, DateTime seenAt)
		=> new()
		{
			ProductId = offer.ProductId,
			Source = offer.Source,
			ForStorage = offer.OfferForStorage,
			CurrencyId = offer.CurrencyId,
			PurchasePrice = offer.PurchasePrice,
			DeliveryTime = GetDeliveryTime(offer, seenAt),
			DeliveryProbability = offer.DeliveryProbability,
			FirstSeenAt = seenAt,
			LastSeenAt = seenAt,
			IsOpen = true
		};

	public bool SameValuesAs(PriceOfferHistory other)
		=> ProductId == other.ProductId &&
			Source == other.Source &&
			ForStorage == other.ForStorage &&
			CurrencyId == other.CurrencyId &&
			PurchasePrice == other.PurchasePrice &&
			DeliveryTime == other.DeliveryTime &&
			DeliveryProbability == other.DeliveryProbability;

	public override long GetId() => Id;

	private static TimeSpan GetDeliveryTime(PriceOffer offer, DateTime seenAt)
		=> offer.DeliveryDate is { } deliveryDate && deliveryDate.Date > seenAt.Date
			? deliveryDate.Date - seenAt.Date
			: TimeSpan.Zero;
}
