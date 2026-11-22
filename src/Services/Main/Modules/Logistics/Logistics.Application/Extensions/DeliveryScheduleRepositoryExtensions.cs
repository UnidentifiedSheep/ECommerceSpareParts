using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Schedule;

namespace Main.Modules.Logistics.Application.Extensions;

public static class DeliveryScheduleRepositoryExtensions
{
	public static async Task<DeliverySchedule> GetRequired(
		this IRepository<DeliverySchedule, DeliveryScheduleKey> repository,
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken)
	{
		var key = new DeliveryScheduleKey(supplier, storageCode.Trim());

		var criteria = Criteria<DeliverySchedule>
			.New()
			.Include(x => x.Exceptions)
			.Track()
			.Where(x => x.Supplier == key.Supplier && x.StorageCode == key.StorageCode)
			.Build();

		return await repository.FirstOrDefaultAsync(criteria, cancellationToken)
			?? throw new DeliveryScheduleNotFoundException(key);
	}
}
