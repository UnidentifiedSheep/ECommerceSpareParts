using Analytics.Enums;

namespace Analytics.Entities;

public class MetricCalculationJob
{
    public Guid RequestId { get; set; }
    public Guid? MetricId { get; set; }
    public string MetricSystemName { get; set; } = null!;
    public CalculationStatus Status { get; set; }
    
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    public uint RowVersion { get; set; }
}