using Application.Common.Interfaces.Lrt;
using Application.Common.Models.Lrt;
using Main.Modules.Jobs.Application.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;

namespace Main.Modules.Jobs.Application.LRT;

public sealed class LrtQuotaRegistry : INamedObjectRegistry<ILrtQuotaManager>, IDisposable
{
	private readonly IReadOnlyDictionary<string, ILrtQuotaManager> _managers;

	public LrtQuotaRegistry(IEnumerable<LrtQuotaRegistration> registrations, IServiceProvider services)
	{
		var signal = services.GetService<IJobSignal>();
		var managers = new Dictionary<string, ILrtQuotaManager>(StringComparer.OrdinalIgnoreCase);

		foreach (var registration in registrations)
		{
			if (managers.ContainsKey(registration.SystemName))
				throw new InvalidOperationException(
					$"LRT quota '{registration.SystemName}' is registered more than once.");

			managers[registration.SystemName] = new LrtQuotaManager(
				registration.SystemName,
				registration.ResolveMaxParallelPerWorker(services),
				signal);
		}

		_managers = managers;
	}

	public IReadOnlyCollection<ILrtQuotaManager> All => _managers.Values.ToList();

	public ILrtQuotaManager GetBySystemName(string systemName)
		=> _managers.TryGetValue(systemName, out var manager)
			? manager
			: throw new KeyNotFoundException($"LRT quota '{systemName}' not found");

	public ILrtQuotaManager? TryGetBySystemName(string systemName)
		=> _managers.GetValueOrDefault(systemName);

	public void Dispose()
	{
		foreach (var manager in _managers.Values.OfType<IDisposable>())
			manager.Dispose();
	}

	INamedObject? INamedObjectRegistry.TryGetBySystemName(string systemName) => TryGetBySystemName(systemName);

	IReadOnlyCollection<INamedObject> INamedObjectRegistry.All => All;

	INamedObject INamedObjectRegistry.GetBySystemName(string systemName) => GetBySystemName(systemName);
}
