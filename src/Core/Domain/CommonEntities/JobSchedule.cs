using System.Linq.Expressions;
using Domain.Extensions;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class JobSchedule : AuditableEntity<JobSchedule, Guid>, ILinqEntity<JobSchedule, Guid>
{
    public const int NameMaxLength = 64;
    public const int DescriptionMaxLength = 255;
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.Utc;
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string JobSystemName { get; private set; } = null!;
    public string InputState { get; private set; } = null!;
    public int MaxAttempts { get; private set; }
    public string Cron { get; private set; } = null!;
    public bool Enabled { get; private set; }
    public DateTime? LastQueuedAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }

    private readonly List<JobScheduleRun> _runs = [];
    public IReadOnlyList<JobScheduleRun> Runs => _runs;

    private JobSchedule() {}
    
    private JobSchedule(
        string name,
        string? description,
        string jobSystemName,
        string inputState,
        int maxAttempts,
        string cron)
    {
        SetName(name);
        SetDescription(description);
        JobSystemName = jobSystemName;
        InputState = inputState;
        SetMaxAttempts(maxAttempts);
        SetCron(cron);
    }
    
    public static JobSchedule Create(
        string name,
        string? description,
        string jobSystemName, 
        string inputState, 
        int maxAttempts, 
        string cron)  
        => new(name, description, jobSystemName, inputState, maxAttempts, cron);

    public void AddScheduleRun(Guid jobId, DateTime scheduledAt, DateTime queuedAt)
        => _runs.Add(JobScheduleRun.Create(Id, jobId, scheduledAt, queuedAt));

    public void SetName(string name)
    {
        Name = name
            .TrimSafe()
            .AgainstNullOrWhiteSpace("job.schedule.name.required")
            .AgainstTooLong(NameMaxLength, "job.schedule.name.max.length");
    }

    public void SetDescription(string? description)
    {
        Description = description
            .NullIfWhiteSpace()?
            .AgainstTooLong(DescriptionMaxLength, "job.schedule.description.max.length");
    }

    public void SetCron(string cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        Cron = cron
            .TrimSafe()
            .AgainstNullOrWhiteSpace(() => throw new InvalidOperationException("Cron cannot be null or empty."));
    }

    public void SetInputState(string inputState)
    {
        InputState = inputState
            .TrimSafe()
            .AgainstNullOrWhiteSpace("job.schedule.input.state.required");
    }

    public void SetMaxAttempts(int maxAttempts)
    {
        MaxAttempts = maxAttempts.AgainstLessOrEqual(
            0,
            "job.max.attempts.must.be.greater.than.zero");
    }

    public void SetNextRunAt(DateTime? nextRunAt)
    {
        NextRunAt = nextRunAt;
    }

    public void MarkQueued(DateTime queuedAt, DateTime? nextRunAt)
    {
        LastQueuedAt = queuedAt;
        NextRunAt = nextRunAt;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void Enable()
    {
        Enabled = true;
    }
    
    public override Guid GetId() => Id;

    public static Expression<Func<JobSchedule, Guid>> GetKeySelector()
        => x => x.Id;

    public static Expression<Func<JobSchedule, bool>> GetEqualityExpression(Guid key)
        => x => x.Id == key;
}
