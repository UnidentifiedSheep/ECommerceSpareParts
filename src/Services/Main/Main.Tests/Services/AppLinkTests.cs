using Application.Common.Interfaces.Links;
using Application.Common.Interfaces.Settings;
using Application.Common.Services.Links;
using Main.Application.Models.Options;
using Main.Application.Services;
using Main.Entities.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Services;

public sealed class AppLinkTests
{
	[Fact]
	public async Task TokenLinks_EscapeReservedCharacters()
	{
		var provider = new AuthLinkProvider(
			CreateBuilder(),
			Options.Create(
				new IdentityLinksOptions
				{
					PasswordReset = "account/reset?code={token}",
					EmailVerification = "account/verify?code={token}"
				}));

		var passwordUrl = await provider.CreatePasswordResetUrlAsync(
			"a+b?c&d",
			TestContext.Current.CancellationToken);
		var verificationUrl = await provider.CreateEmailVerificationUrlAsync(
			"a+b?c&d",
			TestContext.Current.CancellationToken);

		Assert.Equal(
			"https://app.example.com/base/account/reset?code=a%2Bb%3Fc%26d",
			passwordUrl.AbsoluteUri);
		Assert.Equal(
			"https://app.example.com/base/account/verify?code=a%2Bb%3Fc%26d",
			verificationUrl.AbsoluteUri);
	}

	[Fact]
	public async Task DocumentLink_UsesConfiguredTemplate()
	{
		var provider = new DocumentUrlProvider(
			CreateBuilder(),
			Options.Create(new DocumentLinksOptions { Document = "reports/{requestId}/open" }));
		var requestId = Guid.Parse("8b6a183e-c297-42e8-9367-9a04e536cce7");

		var url = await provider.CreateDocumentUrlAsync(requestId, TestContext.Current.CancellationToken);

		Assert.Equal(
			"https://app.example.com/base/reports/8b6a183e-c297-42e8-9367-9a04e536cce7/open",
			url.AbsoluteUri);
	}

	[Fact]
	public async Task Builder_RejectsTemplateMissingPlaceholder()
	{
		var act = () => CreateBuilder().CreateAsync(
			"reports/current",
			new Dictionary<string, string> { ["requestId"] = "1" },
			TestContext.Current.CancellationToken);

		await Assert.ThrowsAsync<InvalidOperationException>(act);
	}

	[Fact]
	public void Options_RejectMissingPlaceholder()
	{
		Assert.True(new IdentityLinksOptions().IsValid());
		Assert.True(new DocumentLinksOptions().IsValid());
		Assert.False(new DocumentLinksOptions { Document = "reports/current" }.IsValid());
		Assert.False(new IdentityLinksOptions { PasswordReset = "/reset?token={token}" }.IsValid());
		Assert.False(AppLinkTemplates.IsValid("https://other.example/{token}", "token"));
	}

	private static IAppLinkBuilder CreateBuilder()
	{
		var settings = new Mock<ISettingsService>();
		settings.Setup(service
				=> service.GetOrDefault<GlobalApplicationSetting>(It.IsAny<CancellationToken>()))
			.ReturnsAsync(
				new GlobalApplicationSetting(
					new GlobalApplicationSettingData
					{
						AppServiceUrl = "https://app.example.com/base"
					}));

		return new AppLinkBuilder(settings.Object);
	}
}
