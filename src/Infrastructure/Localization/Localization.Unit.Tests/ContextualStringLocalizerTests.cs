using System.Globalization;
using FluentAssertions;
using Localization.Domain;
using Microsoft.Extensions.Options;

namespace Localization.Unit.Tests;

public class ContextualStringLocalizerTests
{
	[Fact]
	public void Get_ShouldUseDefaultLocale_WhenCurrentLocaleIsNotSupported()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("de-DE");

		var result = localizer.Get("Test.Key");

		result.Should().Be("значение");
	}

	[Fact]
	public void Get_ShouldReturnValue_WhenLocaleSet()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var result = localizer.Get("Test.Key");

		result.Should().Be("value");
	}

	[Fact]
	public void Get_ShouldReturnFormattedValue_WhenArgumentsProvided()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var result = localizer.Get("Formatted.Key", "World");

		result.Should().Be("Hello, World.");
	}

	[Fact]
	public void Indexer_ShouldWork()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var result = localizer["Test.Key"];

		result.Should().Be("value");
	}

	[Fact]
	public void TryGet_ShouldUseDefaultLocale_WhenCurrentLocaleIsNotSupported()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("de-DE");

		var result = localizer.TryGet("Test.Key", out var value);

		result.Should().BeTrue();
		value.Should().Be("значение");
	}

	[Fact]
	public void TryGet_ShouldReturnTrueAndValue_WhenKeyExists()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var success = localizer.TryGet("Test.Key", out var value);

		success.Should().BeTrue();
		value.Should().Be("value");
	}

	[Fact]
	public void TryGet_ShouldReturnTrueAndFormattedValue_WhenArgumentsProvided()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var success = localizer.TryGet(
			"Formatted.Key",
			out var value,
			"World");

		success.Should().BeTrue();
		value.Should().Be("Hello, World.");
	}

	[Fact]
	public void GetOrDefault_ShouldReturnFormattedValue_WhenArgumentsProvided()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var value = localizer.GetOrDefault("Formatted.Key", "World");

		value.Should().Be("Hello, World.");
	}

	[Fact]
	public void TryGet_ShouldReturnFalseAndNull_WhenKeyDoesNotExist()
	{
		var baseLocalizer = CreateBaseLocalizer();
		var localizer = CreateContextualLocalizer(baseLocalizer);
		using var _ = UseCulture("en-US");

		var success = localizer.TryGet("NonExistent.Key", out var value);

		success.Should().BeFalse();
		value.Should().BeNull();
	}

	[Fact]
	public async Task Get_ShouldIsolateLocalesBetweenParallelAsyncFlows()
	{
		var localizer = CreateContextualLocalizer(CreateBaseLocalizer());

		var english = Task.Run(async () =>
		{
			using var _ = UseCulture("en-US");
			await Task.Yield();
			return localizer.Get("Test.Key");
		});
		var russian = Task.Run(async () =>
		{
			using var _ = UseCulture("ru-RU");
			await Task.Yield();
			return localizer.Get("Test.Key");
		});

		var results = await Task.WhenAll(english, russian);

		results.Should().Equal("value", "значение");
	}

	private static StringLocalizer CreateBaseLocalizer()
	{
		var container = new LocalizerContainer("en");
		container.Initialize(
			new Dictionary<string, string>
			{
				["Test.Key"] = "value", ["Formatted.Key"] = "Hello, {0}."
			});
		var defaultContainer = new LocalizerContainer("ru");
		defaultContainer.Initialize(
			new Dictionary<string, string>
			{
				["Test.Key"] = "значение", ["Formatted.Key"] = "Привет, {0}."
			});

		return new StringLocalizer([container, defaultContainer]);
	}

	private static ContextualStringLocalizer CreateContextualLocalizer(StringLocalizer baseLocalizer)
	{
		return new ContextualStringLocalizer(
			baseLocalizer,
			Options.Create(
				new LocalesOptions
				{
					Default = "ru-RU", Supported = ["ru-RU", "en-US"]
				}));
	}

	private static IDisposable UseCulture(string name)
	{
		var previousCulture = CultureInfo.CurrentCulture;
		var previousUiCulture = CultureInfo.CurrentUICulture;
		var culture = CultureInfo.GetCultureInfo(name);
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;

		return new CultureScope(previousCulture, previousUiCulture);
	}

	private sealed class CultureScope(
		CultureInfo previousCulture,
		CultureInfo previousUiCulture) : IDisposable
	{
		public void Dispose()
		{
			CultureInfo.CurrentCulture = previousCulture;
			CultureInfo.CurrentUICulture = previousUiCulture;
		}
	}
}
