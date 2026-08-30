using System.Reflection;

namespace Localization.Domain.Extensions;

public static class AssemblyExtensions
{
	public static string GetDefaultLocalizationPath(this Assembly assembly) => Path.Combine(
		Path.GetDirectoryName(assembly.Location)!,
		"Localization");
}
