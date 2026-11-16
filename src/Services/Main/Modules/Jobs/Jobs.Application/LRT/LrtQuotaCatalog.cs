using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Jobs.Application.LRT;

public sealed class LrtQuotaCatalog(
	IServiceScopeFactory scopeFactory,
	INamedObjectRegistry<ILrtQuotaManager> quotas)
{
	private readonly Lock _sync = new();

	private IReadOnlyDictionary<string, ILrtQuotaManager>? _managers;

	public IReadOnlyDictionary<string, ILrtQuotaManager> ManagersByLrt
	{
		get
		{
			lock (_sync)
				return _managers ??= Build();
		}
	}

	public void Validate() => _ = ManagersByLrt;

	private Dictionary<string, ILrtQuotaManager> Build()
	{
		using var scope = scopeFactory.CreateScope();
		var lrts = scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>().All;
		var result = new Dictionary<string, ILrtQuotaManager>(StringComparer.OrdinalIgnoreCase);
		var unknown = new List<string>();

		foreach (var lrt in lrts)
		{
			var quotaName = (lrt as IQuotaBoundLrt)?.QuotaSystemName ?? LrtQuotaRegistrations.Default;
			var manager = quotas.TryGetBySystemName(quotaName);
			if (manager is null)
				unknown.Add($"{lrt.SystemName} -> {quotaName}");
			else
				result[lrt.SystemName] = manager;
		}

		if (unknown.Count > 0)
			throw new InvalidOperationException(
				$"LRTs reference unknown quotas: {string.Join(", ", unknown)}.");

		return result;
	}
}
