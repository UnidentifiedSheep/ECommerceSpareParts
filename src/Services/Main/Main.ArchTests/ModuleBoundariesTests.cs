using FluentAssertions;
using NetArchTest.Rules;

namespace Main.ArchTests;

public class ModuleBoundariesTests
{
	private static readonly string[] HostAssemblies =
	[
		"Main.Application",
		"Main.Persistence",
		"Main.Cache",
		"Main.Api",
		"Main.Worker",
		"Main.Migrator",
		"Main.Entities"
	];

	private static readonly Dictionary<string, string[]> ForbiddenLayerReferences = new()
	{
		["Entities"] = ["Abstractions", "Application", "Persistence", "Module"],
		["Abstractions"] = ["Application", "Persistence", "Module"],
		["Application"] = ["Persistence", "Module"],
		["Persistence"] = ["Module"]
	};

	private static readonly Dictionary<string, string[]> AllowedModuleDependencies = new()
	{
		["Identity"] = [],
		["Currency"] = [],
		["Product"] = [],
		["Storage"] = ["Currency", "Product"],
		["Pricing"] = ["Currency", "Product", "Storage"],
		["Organization"] = ["Identity"],
		["Balance"] = ["Currency", "Identity", "Organization"],
		["Cart"] = ["Identity", "Product"],
		["Notifications"] = ["Identity"],
		["Order"] = ["Currency", "Identity", "Organization", "Product"],
		["Purchase"] = ["Balance", "Currency", "Identity", "Organization", "Product", "Storage"],
		["Sale"] = ["Balance", "Currency", "Identity", "Organization", "Product", "Storage"]
	};

	[Fact]
	public void Modules_AreDiscovered()
		=> SolutionLayout.ModuleAssemblies
			.Select(assembly => assembly.Module)
			.Distinct()
			.Should()
			.Contain(["Currency", "Product"]);

	[Fact]
	public void ModuleAssemblies_DoNotReferenceTheHost()
	{
		var violations = SolutionLayout.ModuleAssemblies
			.SelectMany(assembly => assembly.References
				.Where(HostAssemblies.Contains)
				.Select(reference => $"{assembly.Name} -> {reference}"))
			.ToList();

		violations.Should().BeEmpty("modules must depend only on shared/common projects, never on the Main host");
	}

	[Fact]
	public void ModuleLayers_RespectDependencyDirection()
	{
		var violations = new List<string>();
		foreach (var assembly in SolutionLayout.ModuleAssemblies)
		{
			if (!ForbiddenLayerReferences.TryGetValue(assembly.Layer, out var forbiddenLayers))
				continue;

			foreach (var layer in forbiddenLayers)
			{
				var forbidden = $"Main.Modules.{assembly.Module}.{layer}";
				if (assembly.References.Contains(forbidden))
					violations.Add($"{assembly.Name} -> {forbidden}");
			}
		}

		violations.Should().BeEmpty("inner layers must not depend on outer layers");
	}

	[Fact]
	public void Modules_DependOnEachOtherOnlyThroughAllowedEdges()
	{
		var violations = new List<string>();
		foreach (var assembly in SolutionLayout.ModuleAssemblies)
		{
			var allowed = AllowedModuleDependencies.GetValueOrDefault(assembly.Module, []);
			foreach (var reference in assembly.References.Where(x => x.StartsWith("Main.Modules.")))
			{
				var referencedModule = reference.Split('.')[2];
				if (referencedModule != assembly.Module && !allowed.Contains(referencedModule))
					violations.Add($"{assembly.Name} -> {reference}");
			}
		}

		violations.Should().BeEmpty("cross-module references must be declared in AllowedModuleDependencies");
	}

	[Fact]
	public void Modules_ReferenceOtherModulesOnlyThroughAbstractions()
	{
		var violations = SolutionLayout.ModuleAssemblies
			.SelectMany(assembly => assembly.References
				.Where(reference => reference.StartsWith("Main.Modules."))
				.Where(reference =>
				{
					var parts = reference.Split('.');
					return parts[2] != assembly.Module && parts[3] != "Abstractions";
				})
				.Select(reference => $"{assembly.Name} -> {reference}"))
			.ToList();

		violations.Should().BeEmpty("modules may use other modules only through their Abstractions");
	}

	[Fact]
	public void SharedAssemblies_DoNotReferenceMainOrModules()
	{
		string[] shared = ["Application.Common", "Domain", "Exceptions", "Extensions", "Persistence.Common", "Persistence"];

		var violations = shared
			.Select(SolutionLayout.LoadByName)
			.SelectMany(assembly => assembly.GetReferencedAssemblies()
				.Where(reference => reference.Name!.StartsWith("Main.") || reference.Name.StartsWith("Pricing."))
				.Select(reference => $"{assembly.GetName().Name} -> {reference.Name}"))
			.ToList();

		violations.Should().BeEmpty("shared building blocks must not know about concrete services or modules");
	}

	[Fact]
	public void ModuleEntities_DoNotDependOnFrameworksOfOuterLayers()
	{
		var failing = SolutionLayout.ModuleAssemblies
			.Where(assembly => assembly.Layer == "Entities")
			.Select(assembly => new
			{
				assembly.Name,
				Result = Types.InAssembly(assembly.Assembly)
					.That()
					.ResideInNamespaceStartingWith("Main.Entities")
					.ShouldNot()
					.HaveDependencyOnAny("MassTransit", "Microsoft.EntityFrameworkCore", "Application.Common")
					.GetResult()
			})
			.Where(x => !x.Result.IsSuccessful)
			.Select(x => $"{x.Name}: {string.Join(", ", x.Result.FailingTypeNames ?? [])}")
			.ToList();

		failing.Should().BeEmpty("domain entities must stay free of application and infrastructure frameworks");
	}
}
