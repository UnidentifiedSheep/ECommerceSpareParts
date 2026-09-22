namespace Notification.Core;

public sealed record NotificationSendResult
{
	public bool Succeeded { get; }
	public string? Error { get; }

	private NotificationSendResult(bool succeeded, string? error)
	{
		Succeeded = succeeded;
		Error = error;
	}

	public static NotificationSendResult Success() => new(true, null);

	public static NotificationSendResult Failure(string error)
	{
		if (string.IsNullOrWhiteSpace(error))
			throw new ArgumentException("Failure error must not be null or empty.", nameof(error));

		return new NotificationSendResult(false, error);
	}
}
