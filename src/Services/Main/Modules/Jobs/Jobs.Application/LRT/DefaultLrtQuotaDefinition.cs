using Application.Common.Interfaces.Lrt;
using Application.Common.Models.Options;
using Microsoft.Extensions.Options;

namespace Application.Common.LRT;

public sealed class DefaultLrtQuotaDefinition(IOptions<LrtExecutorOptions> options) : ILrtQuota
{
	public string SystemName => DefaultLrtQuota.SystemName;

	public int MaxParallelPerWorker => options.Value.MaxParallelPerWorker;
}
