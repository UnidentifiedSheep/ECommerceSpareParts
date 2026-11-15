using Npgsql;

namespace Persistence.Listening;

internal static class ChannelNames
{
	private const int MaxLength = 63;

	public static string Validate(string channel)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channel);
		if (System.Text.Encoding.UTF8.GetByteCount(channel) > MaxLength)
			throw new ArgumentException(
				$"Channel name must not exceed {MaxLength} bytes.",
				nameof(channel));

		return channel;
	}

	public static string Quote(string channel) => new NpgsqlCommandBuilder().QuoteIdentifier(Validate(channel));
}
