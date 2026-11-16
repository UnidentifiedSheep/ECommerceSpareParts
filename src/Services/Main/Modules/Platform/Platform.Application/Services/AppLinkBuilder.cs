using Application.Common.Interfaces.Links;
using Application.Common.Interfaces.Settings;
using Exceptions;
using Main.Entities;
using Main.Entities.Platform.Settings;

namespace Main.Modules.Platform.Application.Services;

public sealed class AppLinkBuilder(ISettingsService settingsService) : IAppLinkBuilder
{
	public async Task<Uri> CreateAsync(
		string relativeTemplate,
		IReadOnlyDictionary<string, string> parameters,
		CancellationToken cancellationToken = default)
	{
		var relativeUrl = relativeTemplate;
		foreach (var (name, value) in parameters)
		{
			var placeholder = $"{{{name}}}";
			if (!relativeUrl.Contains(placeholder, StringComparison.Ordinal))
				throw new InvalidOperationException($"App link template is missing '{placeholder}'.");

			relativeUrl = relativeUrl.Replace(
				placeholder,
				Uri.EscapeDataString(value),
				StringComparison.Ordinal);
		}

		if (relativeUrl.IndexOfAny(['{', '}']) >= 0)
			throw new InvalidOperationException("App link template contains unresolved placeholders.");

		var setting = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken)).Data;
		var appServiceUrl = setting.AppServiceUrl ??
			throw new InvalidInputException(
				GlobalApplicationSettingAppServiceUrlNotConfiguredMessage.Instance);

		return new Uri(new Uri(appServiceUrl.TrimEnd('/') + "/"), relativeUrl);
	}
}
