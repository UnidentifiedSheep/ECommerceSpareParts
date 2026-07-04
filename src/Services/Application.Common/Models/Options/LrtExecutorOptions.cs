namespace Application.Common.Models.Options;

public class LrtExecutorOptions
{
    public const string SectionName = "LrtExecutor";
    public int MaxParallelPerWorker { get; set; } = 3;
    public int MaxExpiredLeaseFailBatchSize { get; set; } = 100;
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(30);
}