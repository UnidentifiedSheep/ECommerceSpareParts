using System.Globalization;
using Locan.Core.Interfaces.Localizers;

namespace Mailing.Core.Models;

public sealed class LoginNotificationData : IEmailData
{
	public LoginNotificationData(
		IContextualLocalizer localizer,
		DateTime occurredAtUtc,
		string? ipAddress,
		string? userAgent,
		string to)
	{
		Locale = CultureInfo.CurrentUICulture;
		To = to;
		HtmlLang = Locale.ToString().ToLowerInvariant();
		Subject = localizer.Get(new MailLoginNotificationSubjectMessage());
		Title = localizer.Get(new MailLoginNotificationTitleMessage());
		Intro = localizer.Get(new MailLoginNotificationIntroMessage());
		DateTimeLabel = localizer.Get(new MailLoginNotificationDateTimeLabelMessage());
		IpAddressLabel = localizer.Get(new MailLoginNotificationIpAddressLabelMessage());
		DeviceLabel = localizer.Get(new MailLoginNotificationDeviceLabelMessage());
		WasYou = localizer.Get(new MailLoginNotificationWasYouMessage());
		NotYou = localizer.Get(new MailLoginNotificationNotYouMessage());

		var unknown = localizer.Get(new MailLoginNotificationUnknownMessage());
		OccurredAt = FormatOccurredAt(occurredAtUtc, Locale);
		IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? unknown : ipAddress;
		Device = FormatDevice(userAgent, unknown);
	}

	public CultureInfo Locale { get; }

	public string HtmlLang { get; }

	public string Title { get; }

	public string Intro { get; }

	public string DateTimeLabel { get; }

	public string OccurredAt { get; }

	public string IpAddressLabel { get; }

	public string IpAddress { get; }

	public string DeviceLabel { get; }

	public string Device { get; }

	public string WasYou { get; }

	public string NotYou { get; }

	public string TemplateName => "LoginNotification";

	public string Subject { get; }

	public string To { get; }

	private static string FormatOccurredAt(DateTime occurredAtUtc, CultureInfo locale)
	{
		var culture = CultureInfo.GetCultureInfo(locale.ToString());
		return $"{occurredAtUtc.ToUniversalTime().ToString("f", culture)} UTC";
	}

	private static string FormatDevice(string? userAgent, string unknown)
	{
		if (string.IsNullOrWhiteSpace(userAgent))
			return unknown;

		var browser = GetBrowser(userAgent);
		var operatingSystem = GetOperatingSystem(userAgent);
		var parts = new[]
			{
				browser, operatingSystem
			}
			.Where(x => x is not null)
			.ToList();

		return parts.Count > 0 ? string.Join(" · ", parts) :
			userAgent.Length <= 256 ? userAgent : userAgent[..256];
	}

	private static string? GetBrowser(string userAgent)
	{
		if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
			return "Microsoft Edge";
		if (userAgent.Contains("OPR/", StringComparison.OrdinalIgnoreCase))
			return "Opera";
		if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
			return "Firefox";
		if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
			return "Chrome";
		if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
			return "Safari";

		return null;
	}

	private static string? GetOperatingSystem(string userAgent)
	{
		if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
			return "Windows";
		if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
			return "Android";
		if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
			userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
			return "iOS";
		if (userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase))
			return "macOS";
		if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
			return "Linux";

		return null;
	}
}
