using Application.Common.Abstractions;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Domain.CommonEntities.Job;
using Domain.CommonEntities.Job.Events;
using Domain.CommonEnums;

namespace Application.Common.DomainEventHandlers.Jobs;

public sealed class ResumeMultiStepJobHandler(
    IRepository<Job, Guid> jobRepository)
    : BatchableDomainEventHandler<JobStepFinishedDomainEvent>
{
    public override async Task Handle(
        Batch<JobStepFinishedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var parentIds = notification.Items
            .Select(x => x.MultiStepJobId)
            .Distinct()
            .ToList();

        if (parentIds.Count == 0) return;

        var criteria = Criteria<Job>.New()
            .Where(x => parentIds.Contains(x.Id))
            .Track()
            .ForUpdate()
            .Build();

        var jobs = await jobRepository.ListAsync(
            criteria,
            cancellationToken);

        foreach (var multiStepJob in jobs
                     .OfType<MultiStepJob>()
                     .Where(x => x.Status == JobStatus.Waiting))
            multiStepJob.Resume();
    }
}
