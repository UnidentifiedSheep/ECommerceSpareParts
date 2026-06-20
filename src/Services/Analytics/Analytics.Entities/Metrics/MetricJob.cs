using System.Linq.Expressions;
using Domain;
using Domain.CommonEntities;
using Domain.Interfaces;

namespace Analytics.Entities.Metrics;

public class MetricJob : Entity<MetricJob, (Guid, Guid)>, ILinqEntity<MetricJob, (Guid, Guid)>
{
    private MetricJob() { }

    private MetricJob(Guid metricId, Guid jobId)
    {
        if (metricId == Guid.Empty)
            throw new ArgumentException("Metric id must be specified.", nameof(metricId));

        if (jobId == Guid.Empty)
            throw new ArgumentException("Job id must be specified.", nameof(jobId));

        MetricId = metricId;
        JobId = jobId;
    }

    public Guid MetricId { get; private set; }
    public Guid JobId { get; private set; }

    public Metric Metric { get; private set; } = null!;
    public Job Job { get; private set; } = null!;

    public static Expression<Func<MetricJob, (Guid, Guid)>> GetKeySelector()
    {
        return x => ValueTuple.Create(x.MetricId, x.JobId);
    }

    public static Expression<Func<MetricJob, bool>> GetEqualityExpression((Guid, Guid) key)
    {
        return x => x.MetricId == key.Item1 && x.JobId == key.Item2;
    }

    public static MetricJob Create(Guid metricId, Guid jobId)
    {
        return new MetricJob(metricId, jobId);
    }

    public override (Guid, Guid) GetId()
    {
        return (MetricId, JobId);
    }
}
