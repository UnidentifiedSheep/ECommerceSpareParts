using System.Globalization;
using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;

namespace Mailing.Core.Models;

public sealed class LoginNotificationData : IEmailData
{
    public LoginNotificationData(
        IScopedStringLocalizer localizer,
        DateTime occurredAtUtc,
        string? ipAddress,
        string? userAgent,
        string to)
    {
        Locale = localizer.Locale;
        To = to;
        HtmlLang = Locale.ToString().ToLowerInvariant();
        Subject = localizer.Get("mail.login.notification.subject");
        Title = localizer.Get("mail.login.notification.title");
        Intro = localizer.Get("mail.login.notification.intro");
        DateTimeLabel = localizer.Get("mail.login.notification.date.time.label");
        IpAddressLabel = localizer.Get("mail.login.notification.ip.address.label");
        DeviceLabel = localizer.Get("mail.login.notification.device.label");
        WasYou = localizer.Get("mail.login.notification.was.you");
        NotYou = localizer.Get("mail.login.notification.not.you");

        var unknown = localizer.Get("mail.login.notification.unknown");
        OccurredAt = FormatOccurredAt(occurredAtUtc, Locale);
        IpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? unknown
            : ipAddress;
        Device = FormatDevice(userAgent, unknown);
    }

    public Locale Locale { get; }
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

    private static string FormatOccurredAt(
        DateTime occurredAtUtc,
        Locale locale)
    {
        var culture = CultureInfo.GetCultureInfo(locale.ToString());
        return $"{occurredAtUtc.ToUniversalTime().ToString("f", culture)} UTC";
    }

    private static string FormatDevice(
        string? userAgent,
        string unknown)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return unknown;

        var browser = GetBrowser(userAgent);
        var operatingSystem = GetOperatingSystem(userAgent);
        var parts = new[] { browser, operatingSystem }
            .Where(x => x is not null)
            .ToList();

        return parts.Count > 0
            ? string.Join(" · ", parts)
            : userAgent.Length <= 256
                ? userAgent
                : userAgent[..256];
    }

    private static string? GetBrowser(string userAgent)
    {
        if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Microsoft Edge";
        if (userAgent.Contains("OPR/", StringComparison.OrdinalIgnoreCase)) return "Opera";
        if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Firefox";
        if (userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Chrome";
        if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase)) return "Safari";

        return null;
    }

    private static string? GetOperatingSystem(string userAgent)
    {
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase)) return "Windows";
        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase)) return "Android";
        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            return "iOS";
        if (userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase)) return "macOS";
        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return "Linux";

        return null;
    }
}
