using Enums;
using HotChocolate;
using Main.Api.GraphQl.Logistics.DataLoaders;
using Main.Api.GraphQl.Logistics.Extensions;
using Main.Api.GraphQl.Types;
using Main.Entities.Logistics.Exceptions;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;

namespace Main.Api.GraphQl.Logistics.Types;

[GraphQLName("DeliverySchedule")]
public sealed class GqlDeliverySchedule
{
	private DeliveryScheduleDto? _dto;

	public GqlDeliverySchedule(Supplier supplier, string storageCode)
		=> Key = new DeliveryScheduleKey(supplier, storageCode.Trim());

	public GqlDeliverySchedule(DeliveryScheduleDto dto) : this(dto.Supplier, dto.StorageCode) => _dto = dto;

	[GraphQLIgnore]
	public DeliveryScheduleKey Key { get; }

	[GraphQLName("supplier")]
	public Supplier Supplier => Key.Supplier;

	[GraphQLName("storageCode")]
	public string StorageCode => Key.StorageCode;

	[GraphQLName("storage")]
	public GqlStorage Storage => new(Key.StorageCode);

	[GraphQLName("days")]
	public async Task<IReadOnlyList<DayOfWeek>> GetDaysAsync(
		IDeliveryScheduleByKeyDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Days.ToDaysOfWeek();

	[GraphQLName("arrivalTime")]
	public async Task<TimeOnly> GetArrivalTimeAsync(
		IDeliveryScheduleByKeyDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).ArrivalTime;

	[GraphQLName("exceptions")]
	public async Task<IReadOnlyList<DateOnly>> GetExceptionsAsync(
		IDeliveryScheduleByKeyDataLoader loader,
		CancellationToken cancellationToken)
		=> (await GetDtoAsync(loader, cancellationToken)).Exceptions;

	private async Task<DeliveryScheduleDto> GetDtoAsync(
		IDeliveryScheduleByKeyDataLoader loader,
		CancellationToken cancellationToken)
		=> _dto ??= await loader.LoadAsync(Key, cancellationToken)
			?? throw new DeliveryScheduleNotFoundException(Key);
}
