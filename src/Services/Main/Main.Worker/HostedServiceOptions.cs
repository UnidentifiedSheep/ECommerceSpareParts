using System.ComponentModel.DataAnnotations;

namespace Main.Worker;

public class HostedServiceOptions
{
    public const string SectionName = "HostedServiceOptions";

    [Required]
    public required EmailWorkOptions EmailWork { get; set; }
}

public class EmailWorkOptions
{
    [Required]
    public required TimeSpan Delay { get; set; }

    public required int ScheduleAtOnce { get; set; } = 100;
}