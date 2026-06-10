namespace Api.Common.Models.Options;

public class LrtExecutorOptions
{
    public const string SectionName = "LrtExecutor";
    public int MaxParallelPerWorker { get; set; } = Math.Min(Environment.ProcessorCount - 3, 1);
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(30);
}