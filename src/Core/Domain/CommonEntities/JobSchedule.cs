using System.Linq.Expressions;
using Domain.Extensions;
using Domain.Interfaces;

namespace Domain.CommonEntities;

public class JobSchedule : AuditableEntity<JobSchedule, Guid>, ILinqEntity<JobSchedule, Guid>
{
    public const string TimeZone = "UTC";
    public Guid Id { get; private set; }
    public string SystemName { get; private set; } = null!;
    public string InputState { get; private set; } = null!;
    public int MaxAttempts { get; private set; }
    public string Cron { get; private set; } = null!;
    public bool Enabled { get; private set; }
    public DateTime? LastQueuedAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }

    private JobSchedule() {}
    
    private JobSchedule(
        string systemName,
        string inputState,
        int maxAttempts,
        string cron)
    {
        SystemName = systemName;
        InputState = inputState;
        SetMaxAttempts(maxAttempts);
        SetCron(cron);
    }
    
    public static JobSchedule Create(
        string systemName, 
        string inputState, 
        int maxAttempts, 
        string cron) 
        => new(systemName, inputState, maxAttempts, cron);

    public void SetCron(string cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        Cron = cron
            .TrimSafe()
            .AgainstNullOrWhiteSpace(() => throw new InvalidOperationException("Cron cannot be null or empty."));
    }

    public void SetMaxAttempts(int maxAttempts)
    {
        MaxAttempts = maxAttempts.AgainstLessOrEqual(
            0,
            () => throw new InvalidOperationException("job.max.attempts.must.be.greater.than.zero"));
    }

    public void SetNextRunAt(DateTime? nextRunAt)
    {
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
