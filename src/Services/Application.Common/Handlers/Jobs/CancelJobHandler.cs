using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MediatR;
using JobNotFoundException = Application.Common.Exceptions.JobNotFoundException;

namespace Application.Common.Handlers.Jobs;

[Transactional]
public record CancelJobCommand(Guid JobId) : ICommand;

public class CancelJobHandler(
    IRepository<Job, Guid> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelJobCommand>
{
    public async Task<Unit> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Job>.New()
            .Where(x => x.Id == request.JobId)
            .Track()
            .ForUpdate()
            .Build();
        
        var job = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
            ?? throw new JobNotFoundException(request.JobId);

        job.RequestCancellation();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
