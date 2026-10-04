using Application.Common.Interfaces.Settings;
using Main.Application.Models.Options;
using Main.Application.Services;
using Main.Entities.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Services;

public sealed class AppLinkProviderTests
{
	[Fact]
	public async Task TokenLinks_EscapeReservedCharacters()
	{
		var provider = CreateProvider(new AppLinksOptions
		{
			PasswordReset = "account/reset?code={token}",
			EmailVerification = "account/verify?code={token}"
		});

		var passwordUrl = await provider.CreatePasswordResetUrlAsync(
			"a+b?c&d", TestContext.Current.CancellationToken);
		var verificationUrl = await provider.CreateEmailVerificationUrlAsync(
			"a+b?c&d", TestContext.Current.CancellationToken);

		Assert.Equal("https://app.example.com/base/account/reset?code=a%2Bb%3Fc%26d", passwordUrl.AbsoluteUri);
		Assert.Equal("https://app.example.com/base/account/verify?code=a%2Bb%3Fc%26d", verificationUrl.AbsoluteUri);
	}

	[Fact]
	public async Task DocumentLink_UsesConfiguredTemplate()
	{
		var provider = CreateProvider(new AppLinksOptions
		{
			Document = "reports/{requestId}/open"
		});
		var requestId = Guid.Parse("8b6a183e-c297-42e8-9367-9a04e536cce7");

		var url = await provider.CreateDocumentUrlAsync(requestId, TestContext.Current.CancellationToken);

		Assert.Equal(
			"https://app.example.com/base/reports/8b6a183e-c297-42e8-9367-9a04e536cce7/open",
			url.AbsoluteUri);
	}

	[Fact]
	public void Options_RejectMissingPlaceholder()
	{
		var options = new AppLinksOptions { Document = "reports/current" };

		Assert.True(new AppLinksOptions().IsValid());
		Assert.False(options.IsValid());
	}

	private static AppLinkProvider CreateProvider(AppLinksOptions options)
	{
		var settings = new Mock<ISettingsService>();
		settings.Setup(service => service.GetOrDefault<GlobalApplicationSetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new GlobalApplicationSetting(new GlobalApplicationSettingData
			{
				AppServiceUrl = "https://app.example.com/base"
			}));

		return new AppLinkProvider(settings.Object, Options.Create(options));
	}
}
