using System.Linq.Expressions;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class JobScheduleRun : Entity<JobScheduleRun, Guid>, ILinqEntity<JobScheduleRun, Guid>
{
    private JobScheduleRun() { }

    private JobScheduleRun(
        Guid jobScheduleId,
        Guid jobId,
        DateTime scheduledAt,
        DateTime queuedAt)
    {
        SetJobScheduleId(jobScheduleId);
        SetJobId(jobId);
        SetScheduledAt(scheduledAt);
        SetQueuedAt(queuedAt);
    }

    public Guid Id { get; private set; }
    public Guid JobScheduleId { get; private set; }
    public Guid JobId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime QueuedAt { get; private set; }

    public static Expression<Func<JobScheduleRun, Guid>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<JobScheduleRun, bool>> GetEqualityExpression(Guid key)
    {
        return x => x.Id == key;
    }

    public static JobScheduleRun Create(
        Guid jobScheduleId,
        Guid jobId,
        DateTime scheduledAt,
        DateTime queuedAt)
    {
        return new JobScheduleRun(
            jobScheduleId,
            jobId,
            scheduledAt,
            queuedAt);
    }

    public void SetJobScheduleId(Guid jobScheduleId)
    {
        if (jobScheduleId == Guid.Empty)
            throw new ArgumentException("Job schedule id must be specified.", nameof(jobScheduleId));

        JobScheduleId = jobScheduleId;
    }

    public void SetJobId(Guid jobId)
    {
        if (jobId == Guid.Empty) throw new ArgumentException("Job id must be specified.", nameof(jobId));

        JobId = jobId;
    }

    public void SetScheduledAt(DateTime scheduledAt) { ScheduledAt = scheduledAt; }

    public void SetQueuedAt(DateTime queuedAt) { QueuedAt = queuedAt; }

    public override Guid GetId() { return Id; }
}