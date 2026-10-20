using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Common;

public static class ConfigurationBuilderExtensions
{
	private const string Appsettings = "appsettings";
	private static readonly string DefaultConfigsPath =
		Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "configs");

	public static IConfigurationBuilder AddAppSettingsFromJsons(
		this IConfigurationBuilder configuration,
		string? contour,
		string? path = null)
	{
		return configuration.AddConfigsFromJsons(
			Appsettings,
			contour,
			path);
	}

	public static IConfigurationBuilder AddMigratorSettingsFromJsons(
		this IConfigurationBuilder configuration,
		string? contour,
		string? path = null)
	{
		return configuration.AddConfigsFromJsons(
			"migrator",
			contour,
			path ?? Path.Combine(DefaultConfigsPath, "migrator"));
	}

	public static IConfigurationBuilder AddConfigsFromJsons(
		this IConfigurationBuilder configuration,
		string nameStart,
		string purpose,
		string? contour,
		string? path)
	{
		return configuration.AddConfigsFromJsons(
			$"{nameStart}.{purpose}",
			contour,
			path);
	}

	public static IConfigurationBuilder AddConfigsFromJsons(
		this IConfigurationBuilder configuration,
		string nameStart,
		string? contour,
		string? path = null)
	{
		path ??= DefaultConfigsPath;

		if (!Directory.Exists(path))
			return configuration;

		configuration.AddJsonFile(
			Path.Combine(path, $"{nameStart}.json"),
			true,
			false);

		if (!string.IsNullOrWhiteSpace(contour))
			configuration.AddJsonFile(
				Path.Combine(path, $"{nameStart}.{contour}.json"),
				true,
				false);

		return configuration;
	}
}
