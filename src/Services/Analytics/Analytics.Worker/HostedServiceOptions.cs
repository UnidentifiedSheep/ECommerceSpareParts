using System.ComponentModel.DataAnnotations;

namespace Analytics.Worker;

public class HostedServiceOptions
{
    public const string SectionName = "HostedServiceOptions";

    [Required]
    public required RecalculationCheckOptions RecalculationCheck { get; set; }
}

public class RecalculationCheckOptions
{
    [Required]
    public required TimeSpan Delay { get; set; }

    public required int ScheduleAtOnce { get; set; } = 100;
}