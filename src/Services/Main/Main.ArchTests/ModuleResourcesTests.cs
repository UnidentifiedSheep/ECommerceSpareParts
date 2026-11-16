using System.Text.RegularExpressions;
using FluentAssertions;

namespace Main.ArchTests;

public class ModuleResourcesTests
{
	private static readonly string[] HostProjects =
	[
		"Main.Api/Main.Api.csproj",
		"Main.Worker/Main.Worker.csproj",
		"Main.Tests/Main.Tests.csproj"
	];

	public static TheoryData<string> Hosts => new(HostProjects);

	[Theory]
	[MemberData(nameof(Hosts))]
	public void Host_CopiesLocalizationOfEveryWiredModule(string hostProject)
	{
		var mainRoot = Path.Combine(
			SolutionLayout.RepositoryRoot,
			"src",
			"Services",
			"Main");
		var csproj = File.ReadAllText(Path.Combine(mainRoot, hostProject)).Replace('\\', '/');
		var wiredModules = WiredModules(mainRoot);

		var missing = wiredModules
			.Where(module => Directory.Exists(
				Path.Combine(
					SolutionLayout.ModulesRoot,
					module,
					$"{module}.Entities",
					"Localization")))
			.Select(module => $"Modules/{module}/{module}.Entities/Localization/**/*.json")
			.Where(include => !csproj.Contains(include))
			.ToList();

		missing.Should()
			.BeEmpty($"{hostProject} must include every wired module's localization as LocanResource");
	}

	private static IReadOnlyList<string> WiredModules(string mainRoot)
		=> Regex
			.Matches(
				File.ReadAllText(
					Path.Combine(
						mainRoot,
						"Main.Api",
						"Main.Api.csproj")),
				@"Modules[\\/](\w+)[\\/]")
			.Select(match => match.Groups[1].Value)
			.Distinct()
			.ToList();
}
