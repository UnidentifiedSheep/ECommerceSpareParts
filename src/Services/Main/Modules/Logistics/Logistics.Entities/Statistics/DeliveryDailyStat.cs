using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Enums;

namespace Main.Entities.Logistics.Statistics;

public sealed class DeliveryDailyStat : Entity<DeliveryDailyStat, DeliveryDailyStatKey>,
	ILinqEntity<DeliveryDailyStat, DeliveryDailyStatKey>
{
	private DeliveryDailyStat()
	{
	}

	public DateOnly Day { get; private set; }

	public Supplier Supplier { get; private set; }

	public string StorageCode { get; private set; } = string.Empty;

	public int ProductId { get; private set; }

	public int DeliveryDays { get; private set; }

	public int OffersCount { get; private set; }

	public int MaxProbability { get; private set; }

	public int? MinGuaranteedDays { get; private set; }

	public DateTime FirstSeenAt { get; private set; }

	public DateTime LastSeenAt { get; private set; }

	public static Expression<Func<DeliveryDailyStat, DeliveryDailyStatKey>> GetKeySelector()
		=> x => new DeliveryDailyStatKey(x.ProductId, x.Supplier, x.StorageCode, x.Day, x.DeliveryDays);

	public static Expression<Func<DeliveryDailyStat, bool>> GetEqualityExpression(DeliveryDailyStatKey key)
		=> x => x.ProductId == key.ProductId &&
			x.Supplier == key.Supplier &&
			x.StorageCode == key.StorageCode &&
			x.Day == key.Day &&
			x.DeliveryDays == key.DeliveryDays;

	public static DeliveryDailyStat Create(
		Supplier supplier,
		string storageCode,
		int productId,
		int deliveryDays,
		int probability,
		int? guaranteedDays,
		DateTime seenAt)
		=> new()
		{
			Day = DateOnly.FromDateTime(seenAt),
			Supplier = supplier,
			StorageCode = storageCode,
			ProductId = productId,
			DeliveryDays = deliveryDays,
			OffersCount = 1,
			MaxProbability = probability,
			MinGuaranteedDays = guaranteedDays,
			FirstSeenAt = seenAt,
			LastSeenAt = seenAt
		};

	public static DeliveryDailyStat Merge(DeliveryDailyStat left, DeliveryDailyStat right)
		=> new()
		{
			Day = left.Day,
			Supplier = left.Supplier,
			StorageCode = left.StorageCode,
			ProductId = left.ProductId,
			DeliveryDays = left.DeliveryDays,
			OffersCount = left.OffersCount + right.OffersCount,
			MaxProbability = Math.Max(left.MaxProbability, right.MaxProbability),
			MinGuaranteedDays = (left.MinGuaranteedDays, right.MinGuaranteedDays) switch
			{
				({ } l, { } r) => Math.Min(l, r),
				var (l, r) => l ?? r
			},
			FirstSeenAt = left.FirstSeenAt < right.FirstSeenAt ? left.FirstSeenAt : right.FirstSeenAt,
			LastSeenAt = left.LastSeenAt > right.LastSeenAt ? left.LastSeenAt : right.LastSeenAt
		};

	public override DeliveryDailyStatKey GetId() => new(ProductId, Supplier, StorageCode, Day, DeliveryDays);
}

public readonly record struct DeliveryDailyStatKey(
	int ProductId,
	Supplier Supplier,
	string StorageCode,
	DateOnly Day,
	int DeliveryDays) : ICompositeKey
{
	public object[] ToArray() => [ProductId, Supplier, StorageCode, Day, DeliveryDays];
}
