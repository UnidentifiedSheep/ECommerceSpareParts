using Enums;
using Main.Modules.Logistics.Application.Dtos;

namespace Main.Modules.Logistics.Application.Interfaces;

public interface IDeliveryScheduleCacheRepository
{
	Task<DeliveryScheduleDto?> GetAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken = default);

	Task InvalidateAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken = default);
}
