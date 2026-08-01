using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Options;

public record ScheduledJobEnqueuerOptions
{
    public const string SectionName = "ScheduledJobEnqueuer";

    [Range(1, int.MaxValue)]
    public int BatchSize { get; set; } = 100;
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);
}
