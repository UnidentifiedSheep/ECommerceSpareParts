using Application.Common.Dtos;

namespace Application.Common.Interfaces.Services;

public interface IJobScheduleService
{
    Task<Guid> CreateScheduleAsync(
        NewJobScheduleDto newSchedule,
        CancellationToken token = default);

    Task<Guid> UpdateScheduleAsync(
        Guid scheduleId,
        PatchJobScheduleDto patch,
        CancellationToken token = default);

    Task RemoveScheduleAsync(
        Guid scheduleId,
        CancellationToken token = default);
}
