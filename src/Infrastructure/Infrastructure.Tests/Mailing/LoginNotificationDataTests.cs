using Localization.Abstractions.Interfaces;
using Localization.Abstractions.Models;
using Mailing.Core.Models;

namespace Infrastructure.Tests.Mailing;

public class LoginNotificationDataTests
{
    [Fact]
    public void Constructor_LocalizesContentAndFormatsDevice()
    {
        using var localizer = new TestStringLocalizer("en");

        var data = new LoginNotificationData(
            localizer,
            new DateTime(2026, 7, 28, 12, 30, 0, DateTimeKind.Utc),
            "192.0.2.15",
            "Mozilla/5.0 (Windows NT 10.0) Chrome/138.0.0.0 Safari/537.36",
            "user@example.com");

        Assert.Equal("user@example.com", data.To);
        Assert.Equal("192.0.2.15", data.IpAddress);
        Assert.Equal("Chrome · Windows", data.Device);
        Assert.Equal("en", data.HtmlLang);
        Assert.Equal("mail.login.notification.subject", data.Subject);
        Assert.Equal("LoginNotification", data.TemplateName);
    }

    [Fact]
    public void Constructor_WhenRequestContextIsMissing_UsesLocalizedUnknownValue()
    {
        using var localizer = new TestStringLocalizer(
            "en",
            new Dictionary<string, string>
            {
                ["mail.login.notification.unknown"] = "Unknown"
            });

        var data = new LoginNotificationData(
            localizer,
            DateTime.UtcNow,
            null,
            null,
            "user@example.com");

        Assert.Equal("Unknown", data.IpAddress);
        Assert.Equal("Unknown", data.Device);
    }

    private sealed class TestStringLocalizer(
        Locale locale,
        IReadOnlyDictionary<string, string>? values = null)
        : IScopedStringLocalizer
    {
        public Locale Locale { get; private set; } = locale;
        public string this[string key] => Get(key);

        public void SetLocale(Locale value) { Locale = value; }

        public string Get(string key)
        {
            return values?.GetValueOrDefault(key) ?? key;
        }

        public string Get(string key, params object[] arguments)
        {
            return string.Format(Get(key), arguments);
        }

        public bool TryGet(string key, out string? value)
        {
            value = GetOrDefault(key);
            return value is not null;
        }

        public bool TryGet(
            string key,
            out string? value,
            params object[] arguments)
        {
            value = GetOrDefault(key, arguments);
            return value is not null;
        }

        public string? GetOrDefault(string key)
        {
            return values?.GetValueOrDefault(key);
        }

        public string? GetOrDefault(
            string key,
            params object[] arguments)
        {
            var value = GetOrDefault(key);
            return value is null
                ? null
                : string.Format(value, arguments);
        }

        public void Dispose()
        {
        }
    }
}
