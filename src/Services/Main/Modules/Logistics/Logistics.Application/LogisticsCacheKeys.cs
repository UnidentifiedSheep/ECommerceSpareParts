using Enums;

namespace Main.Modules.Logistics.Application;

public static class LogisticsCacheKeys
{
	public static TimeSpan Ttl { get; } = TimeSpan.FromDays(1);

	public static string DeliverySchedule(Supplier supplier, string storageCode)
		=> $"logistics:delivery-schedule:{supplier}:{storageCode}";
}
