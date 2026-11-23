using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Logistics.Application.Queries.DeliverySchedules.GetDeliverySchedules;

public record GetDeliverySchedulesQuery(
	IReadOnlyCollection<Supplier> Suppliers,
	string? StorageCode,
	Pagination Pagination) : IQuery<GetDeliverySchedulesResult>;

public record GetDeliverySchedulesResult(IReadOnlyList<DeliveryScheduleDto> Schedules);

public class GetDeliverySchedulesHandler(
	IReadRepository<DeliverySchedule, DeliveryScheduleKey> repository,
	IProjectionProvider<DeliverySchedule, DeliveryScheduleDto> projection)
	: IQueryHandler<GetDeliverySchedulesQuery, GetDeliverySchedulesResult>
{
	public async Task<GetDeliverySchedulesResult> Handle(
		GetDeliverySchedulesQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query;

		if (request.Suppliers.Count > 0)
			query = query.Where(x => request.Suppliers.Contains(x.Supplier));

		if (!string.IsNullOrWhiteSpace(request.StorageCode))
		{
			var storageCode = request.StorageCode.Trim();
			query = query.Where(x => x.StorageCode == storageCode);
		}

		var schedules = await query
			.OrderBy(x => x.Supplier)
			.ThenBy(x => x.StorageCode)
			.Project(projection)
			.ApplyPagination(request.Pagination)
			.ToListAsync(cancellationToken);

		return new GetDeliverySchedulesResult(schedules);
	}
}
