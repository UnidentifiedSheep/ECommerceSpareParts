using System.Reflection;

namespace Main.ArchTests;

internal static class SolutionLayout
{
	public static readonly string RepositoryRoot = FindRepositoryRoot();

	public static string ModulesRoot
		=> Path.Combine(
			RepositoryRoot,
			"src",
			"Services",
			"Main",
			"Modules");

	public static IReadOnlyList<ModuleAssembly> ModuleAssemblies { get; } = LoadModuleAssemblies();

	public static Assembly LoadByName(string name)
		=> Assembly.Load(AssemblyName.GetAssemblyName(Path.Combine(AppContext.BaseDirectory, name + ".dll")));

	private static string FindRepositoryRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null &&
				!File.Exists(Path.Combine(directory.FullName, "ECommerceSpareParts.sln")))
			directory = directory.Parent;

		return directory?.FullName ?? throw new InvalidOperationException("Solution root was not found.");
	}

	private static List<ModuleAssembly> LoadModuleAssemblies()
		=> Directory
			.GetFiles(AppContext.BaseDirectory, "Main.Modules.*.dll")
			.Select(path => Path.GetFileNameWithoutExtension(path))
			.Order()
			.Select(name =>
			{
				var parts = name.Split('.');
				return new ModuleAssembly(
					parts[2],
					parts[3],
					LoadByName(name));
			})
			.ToList();
}

internal sealed record ModuleAssembly(string Module, string Layer, Assembly Assembly)
{
	public string Name => Assembly.GetName().Name!;

	public IReadOnlyList<string> References
		=> Assembly.GetReferencedAssemblies().Select(reference => reference.Name!).ToList();
}
