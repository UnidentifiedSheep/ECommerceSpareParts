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
public record AddDeliveryScheduleExceptionCommand(Supplier Supplier, string StorageCode, DateOnly Date) : ICommand;

public class AddDeliveryScheduleExceptionHandler(
	IRepository<DeliverySchedule, DeliveryScheduleKey> repository,
	TimeProvider timeProvider) : ICommandHandler<AddDeliveryScheduleExceptionCommand>
{
	public async Task<Unit> Handle(AddDeliveryScheduleExceptionCommand request, CancellationToken cancellationToken)
	{
		var schedule = await repository.GetRequired(request.Supplier, request.StorageCode, cancellationToken);
		schedule.AddException(request.Date, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
		return Unit.Value;
	}
}
