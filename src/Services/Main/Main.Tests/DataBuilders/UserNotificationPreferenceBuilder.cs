using Bogus;
using Main.Entities.User;
using Notification.Core.Recipients;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class UserNotificationPreferenceBuilder(Faker faker)
	: BuilderBase<UserNotificationPreference>(faker)
{
	private Guid _userId = Guid.NewGuid();
	private string _channelName = EmailRecipient.ChannelName;
	private bool _enabled = true;

	public UserNotificationPreferenceBuilder WithUserId(Guid userId)
	{
		_userId = userId;
		return this;
	}

	public UserNotificationPreferenceBuilder WithChannelName(string channelName)
	{
		_channelName = channelName;
		return this;
	}

	public UserNotificationPreferenceBuilder WithEnabled(bool enabled)
	{
		_enabled = enabled;
		return this;
	}

	public override UserNotificationPreference Build()
	{
		var preference = UserNotificationPreference.Create(_userId, _channelName);
		preference.SetEnabled(_enabled);
		return preference;
	}
}
