using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Enums;
using Main.Entities.Logistics.Schedule;
using Main.Modules.Logistics.Application.Extensions;
using MediatR;

namespace Main.Modules.Logistics.Application.Commands.DeliverySchedules;

[AutoSave]
[Transactional]
public record DeleteDeliveryScheduleCommand(Supplier Supplier, string StorageCode) : ICommand;

public class DeleteDeliveryScheduleHandler(
	IRepository<DeliverySchedule, DeliveryScheduleKey> repository,
	IUnitOfWork unitOfWork) : ICommandHandler<DeleteDeliveryScheduleCommand>
{
	public async Task<Unit> Handle(DeleteDeliveryScheduleCommand request, CancellationToken cancellationToken)
	{
		var schedule = await repository.GetRequired(request.Supplier, request.StorageCode, cancellationToken);
		unitOfWork.Remove(schedule);
		return Unit.Value;
	}
}
