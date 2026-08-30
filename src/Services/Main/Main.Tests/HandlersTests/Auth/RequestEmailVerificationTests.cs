using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Exceptions;
using Localization.Abstractions.Interfaces;
using Mailing.Core;
using Mailing.Core.Models;
using Main.Application.Handlers.Auth.EmailVerification;
using Main.Application.Interfaces.Services;
using Main.Application.Interfaces.Services.PayloadProvider;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Main.Entities.User;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Auth;

public class RequestEmailVerificationTests : IntegrationTest
{
	public RequestEmailVerificationTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<UserContextTestContext>();
	}

	[Fact]
	public async Task RequestVerification_ExistingUserEmail_QueuesVerificationEmail()
	{
		const string email = "verification@example.com";
		var user = await CreateUser(email, false);
		var mailingService = new Mock<IMailingService>();
		IEmailData? queuedEmail = null;
		mailingService
			.Setup(x => x.QueueEmailAsync(It.IsAny<IEmailData>(), It.IsAny<CancellationToken>()))
			.Callback((IEmailData data, CancellationToken _) => queuedEmail = data)
			.Returns(Task.CompletedTask);

		var handler = CreateHandler(mailingService.Object);

		await handler.Handle(
			new RequestEmailVerificationCommand(user.Id, "Verification@Example.com"),
			CancellationToken.None);

		var verificationEmail = Assert.IsType<EmailVerificationData>(queuedEmail);
		Assert.Equal(email, verificationEmail.To);
		Assert.StartsWith(
			"https://www.somewebsite.com/verify-email?token=",
			verificationEmail.VerificationUrl);
		mailingService.Verify(
			x => x.QueueEmailAsync(It.IsAny<IEmailData>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task RequestVerification_ConfirmedEmail_DoesNotQueueEmail()
	{
		const string email = "confirmed@example.com";
		var user = await CreateUser(email);
		var mailingService = new Mock<IMailingService>();
		var handler = CreateHandler(mailingService.Object);

		await handler.Handle(new RequestEmailVerificationCommand(user.Id, email), CancellationToken.None);

		mailingService.Verify(
			x => x.QueueEmailAsync(It.IsAny<IEmailData>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task RequestVerification_EmailDoesNotExist_Throws()
	{
		var user = await CreateUser();
		var handler = CreateHandler(Mock.Of<IMailingService>());

		var action = () => handler.Handle(
			new RequestEmailVerificationCommand(user.Id, "missing@example.com"),
			CancellationToken.None);

		await Assert.ThrowsAsync<UserEmailNotFoundException>(action);
	}

	[Fact]
	public async Task RequestVerification_EmailBelongsToAnotherUser_Throws()
	{
		const string email = "other-user@example.com";
		await CreateUser(email, false);
		var requestingUser = await CreateUser();
		var handler = CreateHandler(Mock.Of<IMailingService>());

		var action = () => handler.Handle(
			new RequestEmailVerificationCommand(requestingUser.Id, email),
			CancellationToken.None);

		await Assert.ThrowsAsync<UserEmailNotFoundException>(action);
	}

	[Fact]
	public async Task RequestVerification_AppServiceUrlIsMissing_Throws()
	{
		const string email = "verification@example.com";
		var user = await CreateUser(email, false);
		var settingsService = Scope.ServiceProvider.GetRequiredService<ISettingsService>();
		await settingsService.SetSetting(
			new GlobalApplicationSetting(
				new GlobalApplicationSettingData
				{
					ApiServiceUrl = "https://api.example.com", AppServiceUrl = null
				}));
		var handler = CreateHandler(Mock.Of<IMailingService>());

		var action = () => handler.Handle(
			new RequestEmailVerificationCommand(user.Id, email),
			CancellationToken.None);

		var exception = await Assert.ThrowsAsync<InvalidInputException>(action);
		Assert.Equal("global.application.setting.app.service.url.not.configured", exception.MessageKey);
	}

	private RequestEmailVerificationHandler CreateHandler(IMailingService mailingService)
	{
		return new RequestEmailVerificationHandler(
			Scope.ServiceProvider.GetRequiredService<IRepository<UserEmail, string>>(),
			Scope.ServiceProvider.GetRequiredService<IJsonSigner>(),
			mailingService,
			Scope.ServiceProvider.GetRequiredService<IVerificationPayloadProvider>(),
			Scope.ServiceProvider.GetRequiredService<IContextualStringLocalizer>(),
			Scope.ServiceProvider.GetRequiredService<ISettingsService>());
	}

	private async Task<User> CreateUser(string? email = null, bool isConfirmed = true)
	{
		var builder = new MemberUserBuilder(Faker);
		if (email is not null)
			builder.WithEmail(email, isConfirmed: isConfirmed);

		var user = await builder.BuildAndAddToDb(Context);
		Context.ChangeTracker.Clear();
		return user;
	}
}
