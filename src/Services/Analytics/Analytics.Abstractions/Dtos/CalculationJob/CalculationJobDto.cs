using Analytics.Enums;

namespace Analytics.Abstractions.Dtos.CalculationJob;

public class CalculationJobDto
{
    public Guid RequestId { get; set; }
    public Guid? MetricId { get; set; }
    public string MetricSystemName { get; set; } = null!;
    public CalculationStatus Status { get; set; }
    
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}