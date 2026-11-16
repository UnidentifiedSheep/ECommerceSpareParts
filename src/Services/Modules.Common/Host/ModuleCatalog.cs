using System.Reflection;

namespace Application.Common.Host;

public sealed class ModuleCatalog
{
	private readonly List<ModuleBase> _modules = [];

	public IReadOnlyList<ModuleBase> Modules => _modules;

	public IReadOnlyCollection<Assembly> ConsumerAssemblies
		=> _modules.SelectMany(module => module.ConsumerAssemblies).Distinct().ToArray();

	internal bool Add(ModuleBase module)
	{
		if (_modules.Any(existing => existing.GetType() == module.GetType()))
			return false;

		_modules.Add(module);
		return true;
	}
}
