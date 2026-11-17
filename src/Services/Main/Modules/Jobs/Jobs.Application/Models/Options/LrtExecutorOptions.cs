using System.ComponentModel.DataAnnotations;

namespace Main.Modules.Jobs.Application.Models.Options;

public class LrtExecutorOptions
{
	public const string SectionName = "LrtExecutor";

	[Range(1, int.MaxValue)]
	public int MaxParallelPerWorker { get; set; } = 3;

	[Range(1, int.MaxValue)]
	public int MaxExpiredLeaseFailBatchSize { get; set; } = 100;
}
