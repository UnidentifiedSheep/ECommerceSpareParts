using Application.Common.Interfaces.Settings;
using Exceptions;
using Main.Application.Interfaces.Services;
using Main.Application.Models.Options;
using Main.Entities;
using Main.Entities.Settings;
using Microsoft.Extensions.Options;

namespace Main.Application.Services;

public sealed class AppLinkProvider(
	ISettingsService settingsService,
	IOptions<AppLinksOptions> options) : IAppLinkProvider
{
	public Task<Uri> CreatePasswordResetUrlAsync(
		string token,
		CancellationToken cancellationToken = default)
		=> CreateAsync(options.Value.PasswordReset, new Dictionary<string, string>
		{
			["token"] = token
		}, cancellationToken);

	public Task<Uri> CreateEmailVerificationUrlAsync(
		string token,
		CancellationToken cancellationToken = default)
		=> CreateAsync(options.Value.EmailVerification, new Dictionary<string, string>
		{
			["token"] = token
		}, cancellationToken);

	public Task<Uri> CreateDocumentUrlAsync(
		Guid requestId,
		CancellationToken cancellationToken = default)
		=> CreateAsync(options.Value.Document, new Dictionary<string, string>
		{
			["requestId"] = requestId.ToString("D")
		}, cancellationToken);

	private async Task<Uri> CreateAsync(
		string template,
		IReadOnlyDictionary<string, string> parameters,
		CancellationToken cancellationToken)
	{
		var relativeUrl = template;
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
			throw new InvalidInputException(GlobalApplicationSettingAppServiceUrlNotConfiguredMessage.Instance);

		return new Uri(new Uri(appServiceUrl.TrimEnd('/') + "/"), relativeUrl);
	}
}
