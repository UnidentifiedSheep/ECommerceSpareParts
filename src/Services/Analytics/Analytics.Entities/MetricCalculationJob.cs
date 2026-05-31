using Analytics.Enums;
using Domain;
using Domain.Extensions;

namespace Analytics.Entities;

public class MetricCalculationJob : AuditableEntity<MetricCalculationJob, Guid>
{
    private MetricCalculationJob()
    {
    }

    private MetricCalculationJob(string metricSystemName)
    {
        SetMetricSystemName(metricSystemName);
        Status = CalculationStatus.AwaitingWorker;
    }

    public Guid RequestId { get; private set; }
    public Guid? MetricId { get; private set; }
    public string MetricSystemName { get; private set; } = null!;
    public CalculationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public uint RowVersion { get; private set; }

    public bool IsTerminal =>
        Status is CalculationStatus.Succeeded or CalculationStatus.Failed or CalculationStatus.Cancelled;

    public static MetricCalculationJob Create(string metricSystemName)
    {
        return new MetricCalculationJob(metricSystemName);
    }

    public void Start(Guid metricId)
    {
        EnsureStatus(CalculationStatus.AwaitingWorker);
        SetMetricId(metricId);
        ErrorMessage = null;
        Status = CalculationStatus.Calculating;
    }

    public void Succeed(Guid metricId)
    {
        EnsureStatus(CalculationStatus.Calculating);
        SetMetricId(metricId);
        ErrorMessage = null;
        Status = CalculationStatus.Succeeded;
    }

    public void Fail(Guid? metricId, string? errorMessage)
    {
        if (IsTerminal)
            throw new InvalidOperationException("Terminal calculation job cannot be failed.");

        if (metricId.HasValue)
            SetMetricId(metricId.Value);

        ErrorMessage = errorMessage?.TrimOrNull();
        Status = CalculationStatus.Failed;
    }

    public void Cancel(string? errorMessage = null)
    {
        if (IsTerminal)
            throw new InvalidOperationException("Terminal calculation job cannot be cancelled.");

        ErrorMessage = errorMessage?.TrimOrNull();
        Status = CalculationStatus.Cancelled;
    }

    public void EnsureAwaitingWorker()
    {
        EnsureStatus(CalculationStatus.AwaitingWorker);
    }

    public override Guid GetId()
    {
        return RequestId;
    }

    public void SetMetricId(Guid metricId)
    {
        if (metricId == Guid.Empty)
            throw new ArgumentException("Metric id must be specified.", nameof(metricId));

        if (MetricId.HasValue && MetricId.Value != metricId)
            throw new InvalidOperationException("Metric id can not be changed once set.");

        MetricId = metricId;
    }

    private void SetMetricSystemName(string metricSystemName)
    {
        MetricSystemName = metricSystemName
            .TrimSafe()
            .AgainstNullOrWhiteSpace("metric.system.name.required")
            .AgainstTooLong(128, "metric.system.name.too.long");
    }

    private void EnsureStatus(CalculationStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Calculation job must be in {expected} status, but current status is {Status}.");
    }
}