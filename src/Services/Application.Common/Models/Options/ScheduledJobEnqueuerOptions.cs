namespace Application.Common.Models;

public record ScheduledJobEnqueuerOptions
{
    public const string SectionName = "ScheduledJobEnqueuer";

    public int BatchSize { get; set; } = 100;
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);
}