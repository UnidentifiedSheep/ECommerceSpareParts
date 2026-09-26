using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;
using Main.Entities.DomainEvents.User;

namespace Main.Entities.User;

public class UserNotificationPreference :
	Entity<UserNotificationPreference, UserNotificationPreferenceKey>,
	ILinqEntity<UserNotificationPreference, UserNotificationPreferenceKey>
{
	public Guid UserId { get; private set; }
	public string ChannelName { get; private set; } = null!;
	public bool Enabled { get; private set; }

	private UserNotificationPreference() {}

	private UserNotificationPreference(Guid userId, string channelName)
	{
		UserId = userId;
		ChannelName = channelName.EnsureNotNullOrWhiteSpace(
			() => new InvalidOperationException("Channel name cannot be null or empty."));
	}

	public static UserNotificationPreference Create(Guid userId, string channelName)
		=> new(userId, channelName);

	public void SetEnabled(bool enabled) => Enabled = enabled;
	public void Enable() => Enabled = true;
	public void Disable() => Enabled = false;

	public override void OnCreated() => AddDomainEvent(new UserNotificationPreferenceUpdatedDomainEvent(UserId));

	public override void OnUpdated() => OnCreated();

	public override void OnDeleted() => OnCreated();

	public override UserNotificationPreferenceKey GetId() => new(UserId, ChannelName);
	public static Expression<Func<UserNotificationPreference, UserNotificationPreferenceKey>> GetKeySelector()
		=> x => new UserNotificationPreferenceKey(x.UserId, x.ChannelName);
	public static Expression<Func<UserNotificationPreference, bool>> GetEqualityExpression(UserNotificationPreferenceKey key)
		=> x => x.UserId == key.UserId && x.ChannelName == key.ChannelName;
}

public readonly record struct UserNotificationPreferenceKey(
	Guid UserId,
	string ChannelName) : ICompositeKey
{
	public object[] ToArray() => [UserId, ChannelName];
}
