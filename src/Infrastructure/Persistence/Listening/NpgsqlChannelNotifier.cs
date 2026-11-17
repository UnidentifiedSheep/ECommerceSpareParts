using Microsoft.Extensions.Options;
using Npgsql;

namespace Persistence.Listening;

public sealed class NpgsqlChannelNotifier(IOptions<DatabaseOptions> options) : IChannelNotifier
{
	public async Task NotifyAsync(
		string channel,
		string? payload = null,
		CancellationToken cancellationToken = default)
	{
		ChannelNames.Validate(channel);

		await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
		await connection.OpenAsync(cancellationToken);
		await using var command = new NpgsqlCommand("SELECT pg_notify(@channel, @payload)", connection);
		command.Parameters.AddWithValue("channel", channel);
		command.Parameters.AddWithValue("payload", payload ?? string.Empty);
		await command.ExecuteNonQueryAsync(cancellationToken);
	}
}
