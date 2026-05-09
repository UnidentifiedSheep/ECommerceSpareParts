using Analytics.Enums;
using Domain;

namespace Analytics.Entities;

public class MetricCalculationJob : AuditableEntity<MetricCalculationJob, Guid>
{
    public Guid RequestId { get; set; }
    public Guid? MetricId { get; set; }
    public string MetricSystemName { get; set; } = null!;
    public CalculationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public uint RowVersion { get; set; }
    public override Guid GetId() => RequestId;
}