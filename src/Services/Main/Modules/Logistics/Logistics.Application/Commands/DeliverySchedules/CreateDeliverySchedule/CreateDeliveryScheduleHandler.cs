using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Enums;
using Main.Entities.Logistics.Enums;
using Main.Entities.Logistics.Schedule;
using MediatR;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules.CreateDeliverySchedule;

[AutoSave]
[Transactional]
public record CreateDeliveryScheduleCommand(Supplier Supplier, string StorageCode, DeliveryDays Days) : ICommand;

public class CreateDeliveryScheduleHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateDeliveryScheduleCommand>
{
	public async Task<Unit> Handle(CreateDeliveryScheduleCommand request, CancellationToken cancellationToken)
	{
		var schedule = DeliverySchedule.Create(request.Supplier, request.StorageCode, request.Days);
		await unitOfWork.AddAsync(schedule, cancellationToken);
		return Unit.Value;
	}
}
