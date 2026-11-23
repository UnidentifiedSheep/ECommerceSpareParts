using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;

namespace Main.Modules.Logistics.Application;

[Lifetime(Lifetime.Singleton)]
public sealed class DeliveryScheduleDtoProjectionProvider : ProjectionProviderBase<DeliverySchedule, DeliveryScheduleDto>
{
	public override Expression<Func<DeliverySchedule, DeliveryScheduleDto>> Projection { get; } = x
		=> new DeliveryScheduleDto
		{
			Supplier = x.Supplier,
			StorageCode = x.StorageCode,
			Days = x.Days,
			Exceptions = x.Exceptions
				.OrderBy(exception => exception.Date)
				.Select(exception => exception.Date)
				.ToList()
		};
}
