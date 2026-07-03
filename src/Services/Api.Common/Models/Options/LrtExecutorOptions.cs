namespace Api.Common.Models.Options;

public class LrtExecutorOptions
{
    public const string SectionName = "LrtExecutor";
    public int MaxParallelPerWorker { get; set; } = 1;
    public int MaxExpiredLeaseFailBatchSize { get; set; } = 100;
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(30);
}