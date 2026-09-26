using FluentAssertions;
using FluentValidation;
using Main.Application.Dtos.NotificationPreference;
using Main.Application.Handlers.NotificationPreferences.UpsertNotificationPreferences;
using Main.Entities;
using Microsoft.EntityFrameworkCore;
using Notification.Core.Recipients;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Notification;

public class UpsertNotificationPreferencesTests : IntegrationTest
{
	public UpsertNotificationPreferencesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<NotificationPreferencesTestContext>();
	}

	private NotificationPreferencesTestContext PreferencesContext =>
		GetContext<NotificationPreferencesTestContext>();

	[Fact]
	public async Task UpsertPreferences_ChangesOnlyRequestedUsersPreference()
	{
		var user = PreferencesContext.Users[0];
		var otherUser = PreferencesContext.Users[1];

		await Mediator.Send(new UpsertNotificationPreferencesCommand(user.Id,
			[new UpsertUserNotificationPreferenceDto
			{
				ChannelName = EmailRecipient.ChannelName,
				IsEnabled = true
			}]), CancellationToken);

		var preferences = await Context.UserNotificationPreferences
			.AsNoTracking()
			.Where(x => x.ChannelName == EmailRecipient.ChannelName)
			.ToArrayAsync(CancellationToken);

		preferences.Should().HaveCount(2);
		preferences.Single(x => x.UserId == user.Id).Enabled.Should().BeTrue();
		preferences.Single(x => x.UserId == otherUser.Id).Enabled.Should().BeTrue();
	}

	[Fact]
	public async Task UpsertPreferences_CreatesMissingPreference()
	{
		var user = PreferencesContext.Users[2];

		await Mediator.Send(new UpsertNotificationPreferencesCommand(user.Id,
			[new UpsertUserNotificationPreferenceDto
			{
				ChannelName = EmailRecipient.ChannelName,
				IsEnabled = false
			}]), CancellationToken);

		var preference = await Context.UserNotificationPreferences
			.AsNoTracking()
			.SingleAsync(x => x.UserId == user.Id && x.ChannelName == EmailRecipient.ChannelName,
				CancellationToken);

		preference.Enabled.Should().BeFalse();
	}

	[Fact]
	public async Task UpsertPreferences_DisablesExistingPreference()
	{
		var user = PreferencesContext.Users[1];
		var otherUser = PreferencesContext.Users[0];

		await Mediator.Send(new UpsertNotificationPreferencesCommand(user.Id,
			[new UpsertUserNotificationPreferenceDto
			{
				ChannelName = EmailRecipient.ChannelName,
				IsEnabled = false
			}]), CancellationToken);

		var preferences = await Context.UserNotificationPreferences
			.AsNoTracking()
			.ToArrayAsync(CancellationToken);

		preferences.Should().HaveCount(2);
		preferences.Single(x => x.UserId == user.Id).Enabled.Should().BeFalse();
		preferences.Single(x => x.UserId == otherUser.Id).Enabled.Should().BeFalse();
	}

	[Fact]
	public async Task UpsertPreferences_KeepsInAppEnabled()
	{
		var user = PreferencesContext.Users[2];

		await Mediator.Send(new UpsertNotificationPreferencesCommand(user.Id,
			[new UpsertUserNotificationPreferenceDto
			{
				ChannelName = InAppRecipient.ChannelName,
				IsEnabled = false
			}]), CancellationToken);

		var preference = await Context.UserNotificationPreferences
			.AsNoTracking()
			.SingleAsync(x => x.UserId == user.Id && x.ChannelName == InAppRecipient.ChannelName,
				CancellationToken);

		preference.Enabled.Should().BeTrue();
	}

	[Fact]
	public async Task UpsertPreferences_WithEmptyCollection_DoesNotChangePreferences()
	{
		var user = PreferencesContext.Users[0];

		await Mediator.Send(new UpsertNotificationPreferencesCommand(user.Id,
			[]), CancellationToken);

		var preferences = await Context.UserNotificationPreferences
			.AsNoTracking()
			.ToArrayAsync(CancellationToken);

		preferences.Should().HaveCount(PreferencesContext.Preferences.Count);
		preferences.Single(x => x.UserId == user.Id).Enabled.Should().BeFalse();
	}

	[Fact]
	public async Task UpsertPreferences_WithDuplicateChannels_ReturnsLocalizedValidationError()
	{
		var user = PreferencesContext.Users[2];
		var command = new UpsertNotificationPreferencesCommand(user.Id,
			[
				new UpsertUserNotificationPreferenceDto
				{
					ChannelName = EmailRecipient.ChannelName,
					IsEnabled = true
				},
				new UpsertUserNotificationPreferenceDto
				{
					ChannelName = EmailRecipient.ChannelName,
					IsEnabled = false
				}
			]);

		var exception = await Assert.ThrowsAsync<ValidationException>(() =>
			Mediator.Send(command, CancellationToken));

		exception.Errors.Should().ContainSingle(x =>
			x.ErrorCode == NotificationsChannelDuplicateMessage.Key);
		(await Context.UserNotificationPreferences.AsNoTracking()
			.CountAsync(x => x.UserId == user.Id, CancellationToken)).Should().Be(0);
	}
}
