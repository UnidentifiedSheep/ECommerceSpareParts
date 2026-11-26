using Application.Common.Extensions;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;
using Main.Modules.Logistics.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Modules.Logistics.Application.Cache;

public sealed class DeliveryScheduleCacheRepository(
	IFusionCache cache,
	IReadRepository<DeliverySchedule, DeliveryScheduleKey> repository,
	IProjectionProvider<DeliverySchedule, DeliveryScheduleDto> projection) : IDeliveryScheduleCacheRepository
{
	public async Task<DeliveryScheduleDto?> GetAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken = default)
	{
		var code = storageCode.Trim();

		return await cache.GetOrSetAsync<DeliveryScheduleDto?>(
			LogisticsCacheKeys.DeliverySchedule(supplier, code),
			token => LoadAsync(supplier, code, token),
			new FusionCacheEntryOptions(LogisticsCacheKeys.Ttl),
			cancellationToken);
	}

	public async Task InvalidateAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken = default)
		=> await cache.RemoveAsync(
			LogisticsCacheKeys.DeliverySchedule(supplier, storageCode.Trim()),
			token: cancellationToken);

	private async Task<DeliveryScheduleDto?> LoadAsync(
		Supplier supplier,
		string storageCode,
		CancellationToken cancellationToken)
		=> await repository.Query
			.Where(x => x.Supplier == supplier && x.StorageCode == storageCode)
			.Project(projection)
			.FirstOrDefaultAsync(cancellationToken);
}
