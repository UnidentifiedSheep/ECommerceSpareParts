using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Domain.CommonEntities.Job;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.JobSchedules.GetSchedule;

public record GetScheduleByIdQuery(Guid ScheduleId)
    : IQuery<GetScheduleByIdResult>;

public record GetScheduleByIdResult(JobScheduleDto Schedule);

public class GetScheduleByIdHandler(
    IReadRepository<JobSchedule, Guid> repository,
    IProjectionProvider<JobSchedule, JobScheduleDto> projection)
    : IQueryHandler<GetScheduleByIdQuery, GetScheduleByIdResult>
{
    public async Task<GetScheduleByIdResult> Handle(
        GetScheduleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var schedule = await repository.Query
            .Where(x => x.Id == request.ScheduleId)
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new JobScheduleNotFoundException(request.ScheduleId);

        return new GetScheduleByIdResult(schedule);
    }
}
