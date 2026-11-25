using Enums;
using GraphQL.Common.Attributes;
using HotChocolate;
using Main.Api.GraphQl.Logistics.Extensions;
using Main.Api.GraphQl.Logistics.Types;
using Main.Api.GraphQl.Logistics.Types.Inputs;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules;
using Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;
using MediatR;

namespace Main.Api.GraphQl.Logistics.Mutations;

public sealed class DeliveryScheduleMutations
{
	[GraphQLName("create")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_EDIT)]
	public async Task<GqlDeliverySchedule> CreateAsync(
		GqlCreateDeliveryScheduleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new CreateDeliveryScheduleCommand(
				input.Supplier,
				input.StorageCode,
				input.Days.ToDeliveryDays(),
				input.ArrivalTime),
			cancellationToken);
		return new GqlDeliverySchedule(input.Supplier, input.StorageCode);
	}

	[GraphQLName("update")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_EDIT)]
	public async Task<GqlDeliverySchedule> UpdateAsync(
		GqlUpdateDeliveryScheduleInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new UpdateDeliveryScheduleCommand(
				input.Supplier,
				input.StorageCode,
				input.Days.ToDeliveryDays(),
				input.ArrivalTime),
			cancellationToken);
		return new GqlDeliverySchedule(input.Supplier, input.StorageCode);
	}

	[GraphQLName("delete")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_EDIT)]
	public async Task<bool> DeleteAsync(
		Supplier supplier,
		string storageCode,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteDeliveryScheduleCommand(supplier, storageCode), cancellationToken);
		return true;
	}

	[GraphQLName("addException")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_EDIT)]
	public async Task<GqlDeliverySchedule> AddExceptionAsync(
		GqlDeliveryScheduleExceptionInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new AddDeliveryScheduleExceptionCommand(input.Supplier, input.StorageCode, input.Date),
			cancellationToken);
		return new GqlDeliverySchedule(input.Supplier, input.StorageCode);
	}

	[GraphQLName("removeException")]
	[RequireAllPermissions(PermissionCodes.DELIVERY_SCHEDULES_EDIT)]
	public async Task<GqlDeliverySchedule> RemoveExceptionAsync(
		GqlDeliveryScheduleExceptionInput input,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new RemoveDeliveryScheduleExceptionCommand(input.Supplier, input.StorageCode, input.Date),
			cancellationToken);
		return new GqlDeliverySchedule(input.Supplier, input.StorageCode);
	}
}
