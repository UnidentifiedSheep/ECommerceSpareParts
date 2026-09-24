using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces.Localizers;
using Notification.Core.Interfaces.Notification;

namespace Main.Application.Notifications;

public class UserLoggedInNotification : INotification<UserLoggedInNotificationData>, ITextNotification
{
	public UserLoggedInNotification(UserLoggedInNotificationData model)
	{
		Model = model;
	}

	public string SystemName => "UserLoggedIn";
	public UserLoggedInNotificationData Model { get; }
}

public record UserLoggedInNotificationData : INotificationModel, INotificationModelWithTitle
{
	public required string Title { get; init; }
	public required string Heading { get; init; }
	public required string HtmlLang { get; init; }
	public required string Intro { get; init; }
	public required string DateTimeLabel { get; init; }
	public required string OccurredAt { get; init; }
	public required string IpAddressLabel { get; init; }
	public required string IpAddress { get; init; }
	public required string DeviceLabel { get; init; }
	public required string Device { get; init; }
	public required string WasYou { get; init; }
	public required string NotYou { get; init; }
	public required string AsText { get; init; }

	public UserLoggedInNotificationData() { }

	[SetsRequiredMembers]
	public UserLoggedInNotificationData(
		IContextualLocalizer localizer,
		DateTime occurredAtUtc,
		string? ipAddress,
		string? userAgent)
	{
		var culture = CultureInfo.CurrentUICulture;
		var unknown = localizer.Get(new NotificationsLoginNotificationUnknownMessage());

		Title = localizer.Get(new NotificationsLoginNotificationSubjectMessage());
		Heading = localizer.Get(new NotificationsLoginNotificationTitleMessage());
		HtmlLang = culture.Name.ToLowerInvariant();
		Intro = localizer.Get(new NotificationsLoginNotificationIntroMessage());
		DateTimeLabel = localizer.Get(new NotificationsLoginNotificationDateTimeLabelMessage());
		OccurredAt = $"{occurredAtUtc.ToUniversalTime().ToString("f", culture)} UTC";
		IpAddressLabel = localizer.Get(new NotificationsLoginNotificationIpAddressLabelMessage());
		IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? unknown : ipAddress;
		DeviceLabel = localizer.Get(new NotificationsLoginNotificationDeviceLabelMessage());
		Device = FormatDevice(userAgent, unknown);
		WasYou = localizer.Get(new NotificationsLoginNotificationWasYouMessage());
		NotYou = localizer.Get(new NotificationsLoginNotificationNotYouMessage());
		AsText = $"{Heading}. {Intro} {DateTimeLabel}: {OccurredAt}; " +
			$"{IpAddressLabel}: {IpAddress}; {DeviceLabel}: {Device}. {NotYou}";
	}

	private static string FormatDevice(string? userAgent, string unknown)
	{
		if (string.IsNullOrWhiteSpace(userAgent))
			return unknown;

		var browser = GetBrowser(userAgent);
		var operatingSystem = GetOperatingSystem(userAgent);
		var parts = new[] { browser, operatingSystem }
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
