using Abstractions.Interfaces.Persistence;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using MediatR;

namespace Application.Common.Handlers.JobSchedules;

[Transactional, AutoSave, Diagnostics]
public record RemoveJobScheduleCommand(Guid JobScheduleId) : ICommand;

public class RemoveJobScheduleHandler(
    IRepository<JobSchedule, Guid> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveJobScheduleCommand>
{
    public async Task<Unit> Handle(RemoveJobScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await repository.GetById(request.JobScheduleId, cancellationToken)
                       ?? throw new JobScheduleNotFoundException(request.JobScheduleId);
        unitOfWork.Remove(schedule);
        return Unit.Value;
    }
}