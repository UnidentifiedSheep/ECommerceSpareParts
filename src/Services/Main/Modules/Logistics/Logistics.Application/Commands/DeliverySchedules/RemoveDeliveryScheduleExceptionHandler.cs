using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Extensions;
using MediatR;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules;

[AutoSave]
[Transactional]
public record RemoveDeliveryScheduleExceptionCommand(Supplier Supplier, string StorageCode, DateOnly Date) : ICommand;

public class RemoveDeliveryScheduleExceptionHandler(IRepository<DeliverySchedule, DeliveryScheduleKey> repository)
	: ICommandHandler<RemoveDeliveryScheduleExceptionCommand>
{
	public async Task<Unit> Handle(
		RemoveDeliveryScheduleExceptionCommand request,
		CancellationToken cancellationToken)
	{
		var schedule = await repository.GetRequired(request.Supplier, request.StorageCode, cancellationToken);
		schedule.RemoveException(request.Date);
		return Unit.Value;
	}
}
