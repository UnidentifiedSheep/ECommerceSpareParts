using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities;
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
}
