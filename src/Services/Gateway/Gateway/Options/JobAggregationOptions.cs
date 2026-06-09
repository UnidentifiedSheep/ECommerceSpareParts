using System.ComponentModel.DataAnnotations;

namespace Gateway.Options;

public sealed class JobAggregationOptions
{
    public const string SectionName = "Gateway:Jobs";
    
    [Required]
    public required IReadOnlyList<JobServiceOptions> Services { get; init; } = [];

    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(5);
}

public sealed class JobServiceOptions
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required Uri Url { get; init; }
}
