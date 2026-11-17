namespace Application.Common.Models.Lrt;

public sealed record LrtQuotaRegistration(string SystemName,
	Func<IServiceProvider, int> ResolveMaxParallelPerWorker);
