using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Logistics.DataLoaders;
using Main.Api.GraphQl.Logistics.Types;
using Main.Api.GraphQl.Logistics.Types.Inputs;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Queries.DeliverySchedules.GetDeliverySchedules;
using MediatR;

namespace Main.Api.GraphQl.Logistics.Queries;

public sealed class DeliveryScheduleQueries
{
	[GraphQLName("byKey")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_GET)]
	public async Task<GqlDeliverySchedule?> GetByKeyAsync(
		Supplier supplier,
		string storageCode,
		IDeliveryScheduleByKeyDataLoader loader,
		CancellationToken cancellationToken)
	{
		var schedule = await loader.LoadAsync(new DeliveryScheduleKey(supplier, storageCode.Trim()), cancellationToken);
		return schedule is null ? null : new GqlDeliverySchedule(schedule);
	}

	[GraphQLName("search")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_GET)]
	public async Task<IReadOnlyList<GqlDeliverySchedule>> SearchAsync(
		GqlSearchDeliverySchedulesInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetDeliverySchedulesQuery(input.Suppliers, input.StorageCode, input.Pagination),
			cancellationToken);
		return result.Schedules.Select(schedule => new GqlDeliverySchedule(schedule)).ToArray();
	}
}
