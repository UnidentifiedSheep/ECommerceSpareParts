using Bogus;
using Main.Entities.Notification;
using Tests.Abstractions;

namespace Tests.DataBuilders;

public class InAppNotificationBuilder(Faker faker) : BuilderBase<InAppNotification>(faker)
{
	private bool _seen;
	private string? _text;
	private Guid? _userId;

	public InAppNotificationBuilder WithUserId(Guid userId)
	{
		_userId = userId;
		return this;
	}

	public InAppNotificationBuilder WithText(string text)
	{
		_text = text;
		return this;
	}

	public InAppNotificationBuilder WithSeen(bool seen = true)
	{
		_seen = seen;
		return this;
	}

	public override InAppNotification Build()
	{
		var notification = InAppNotification.Create(
			_userId ?? Guid.NewGuid(),
			_text ?? Faker.Lorem.Sentence());

		if (_seen)
			notification.See();

		return notification;
	}
}
