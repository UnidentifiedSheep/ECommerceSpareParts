using System.Globalization;
using Enums;
using FluentAssertions;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.LocalizableMessages;
using Main.Entities;
using Main.Entities.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.LocalizationTests;

public class IdentityModuleLocalizationTests : ServiceTest
{
	[Fact]
	public void AuthMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(UserNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void NotificationMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(NotificationsPasswordResetSubjectMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void LrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Services.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtUserTokensClearExpiredNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Theory]
	[InlineData("en")]
	[InlineData("ru")]
	[InlineData("tr")]
	public void EveryPermission_HasNameAndDescription(string culture)
	{
		var localizer = Services.GetRequiredService<ILocalizer>();
		var cultureInfo = CultureInfo.GetCultureInfo(culture);

		var missing = Enum.GetValues<PermissionCodes>()
			.SelectMany(code => new[]
			{
				Permission.GetLocalizationNameKey(code),
				Permission.GetLocalizationDescriptionKey(code)
			})
			.Where(key => !Resolves(localizer, key, cultureInfo))
			.ToList();

		missing.Should().BeEmpty();
	}

	private static bool Resolves(ILocalizer localizer, string key, CultureInfo culture)
	{
		try
		{
			return !string.IsNullOrWhiteSpace(localizer.Get(new LocalizableMessage(key), culture));
		}
		catch (MessageTemplateNotFoundException)
		{
			return false;
		}
	}
}
