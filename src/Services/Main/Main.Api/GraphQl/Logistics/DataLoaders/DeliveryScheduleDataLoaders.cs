using GreenDonut;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;
using Main.Modules.Logistics.Application.Queries.DeliverySchedules;
using MediatR;

namespace Main.Api.GraphQl.Logistics.DataLoaders;

public static class DeliveryScheduleDataLoaders
{
	[DataLoader]
	public static async Task<IReadOnlyDictionary<DeliveryScheduleKey, DeliveryScheduleDto>> GetDeliveryScheduleByKeyAsync(
		IReadOnlyList<DeliveryScheduleKey> keys,
		ISender sender,
		CancellationToken cancellationToken)
		=> (await sender.Send(new GetDeliverySchedulesByKeysQuery(keys), cancellationToken)).Schedules;
}
