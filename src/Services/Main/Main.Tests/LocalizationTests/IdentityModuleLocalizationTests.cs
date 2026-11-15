using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;

namespace Tests.LocalizationTests;

public class IdentityModuleLocalizationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void AuthMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(UserNotFoundMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void NotificationMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(NotificationsPasswordResetSubjectMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void LrtMessages_AreResolvedFromModuleResources()
	{
		var localizer = Scope.ServiceProvider.GetRequiredService<IContextualLocalizer>();

		localizer.TryGet(LrtUserTokensClearExpiredNameMessage.Instance, out var text).Should().BeTrue();
		text.Should().NotBeNullOrWhiteSpace();
	}
}
