using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Extensions;
using MediatR;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules;

[AutoSave]
[Transactional]
public record UpdateDeliveryScheduleCommand(
	Supplier Supplier,
	string StorageCode,
	DeliveryDays Days,
	TimeOnly ArrivalTime) : ICommand;

public class UpdateDeliveryScheduleHandler(IRepository<DeliverySchedule, DeliveryScheduleKey> repository)
	: ICommandHandler<UpdateDeliveryScheduleCommand>
{
	public async Task<Unit> Handle(UpdateDeliveryScheduleCommand request, CancellationToken cancellationToken)
	{
		var schedule = await repository.GetRequired(request.Supplier, request.StorageCode, cancellationToken);
		schedule.SetDays(request.Days);
		schedule.SetArrivalTime(request.ArrivalTime);
		return Unit.Value;
	}
}
