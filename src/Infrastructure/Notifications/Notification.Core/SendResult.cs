namespace Notification.Core;

public sealed record SendResult
{
	public bool Succeeded { get; }
	public string? Error { get; }

	private SendResult(bool succeeded, string? error)
	{
		Succeeded = succeeded;
		Error = error;
	}

	public static SendResult Success() => new(true, null);

	public static SendResult Failure(string error)
	{
		if (string.IsNullOrWhiteSpace(error))
			throw new ArgumentException("Failure error must not be null or empty.", nameof(error));

		return new SendResult(false, error);
	}
}
