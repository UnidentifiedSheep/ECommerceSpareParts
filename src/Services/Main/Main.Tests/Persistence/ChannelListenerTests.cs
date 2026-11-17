using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Persistence;
using Persistence.Listening;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public class ChannelListenerTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

	[Fact]
	public async Task Listen_FirstItemIsResyncAfterListenIsActive()
	{
		await using var hub = CreateHub();
		await using var listener = await hub.ListenAsync("tests_resync", CancellationToken);

		var first = await ReadAsync(listener, 1);

		first.Should().ContainSingle().Which.IsResync.Should().BeTrue();
	}

	[Fact]
	public async Task Notify_DeliversPayloadToSubscriber()
	{
		await using var hub = CreateHub();
		await using var listener = await hub.ListenAsync("tests_payload", CancellationToken);
		var notifier = new NpgsqlChannelNotifier(Options());

		await notifier.NotifyAsync(
			"tests_payload",
			"hello",
			CancellationToken);

		var received = await ReadAsync(listener, 2);
		received.Should().Contain(x => !x.IsResync && x.Payload == "hello");
	}

	[Fact]
	public async Task Notify_DeliversToEverySubscriberOfTheSameChannelOverOneConnection()
	{
		await using var hub = CreateHub();
		await using var first = await hub.ListenAsync("tests_shared", CancellationToken);
		await using var second = await hub.ListenAsync("tests_shared", CancellationToken);
		await using var other = await hub.ListenAsync("tests_other", CancellationToken);
		var notifier = new NpgsqlChannelNotifier(Options());

		await notifier.NotifyAsync(
			"tests_shared",
			"one",
			CancellationToken);

		(await ReadAsync(first, 2)).Should().Contain(x => x.Payload == "one");
		(await ReadAsync(second, 2)).Should().Contain(x => x.Payload == "one");
		(await ReadAsync(
				other,
				2,
				TimeSpan.FromSeconds(1))).Should()
			.NotContain(x => x.Payload == "one");
		(await CountListenerConnectionsAsync()).Should().Be(1);
	}

	[Fact]
	public async Task Dispose_StopsListeningToTheChannelWhenLastSubscriberLeaves()
	{
		await using var hub = CreateHub();
		var listener = await hub.ListenAsync("tests_unlisten", CancellationToken);
		await listener.DisposeAsync();

		await WaitUntilAsync(async () => await CountListenerConnectionsAsync() == 0);
		(await CountListenerConnectionsAsync()).Should().Be(0);
		var notifier = new NpgsqlChannelNotifier(Options());
		await notifier.NotifyAsync(
			"tests_unlisten",
			"ignored",
			CancellationToken);

		var remaining = await ReadAsync(
			listener,
			5,
			TimeSpan.FromSeconds(1));
		remaining.Should().NotContain(x => x.Payload == "ignored");
	}

	[Fact]
	public async Task ConnectionLoss_ReconnectsAndEmitsResync()
	{
		await using var hub = CreateHub();
		await using var listener = await hub.ListenAsync("tests_reconnect", CancellationToken);
		(await ReadAsync(listener, 1)).Should().ContainSingle();

		await TerminateListenerConnectionsAsync();

		var resync = await ReadAsync(
			listener,
			1,
			TimeSpan.FromSeconds(30));
		resync.Should().ContainSingle().Which.IsResync.Should().BeTrue();
		await new NpgsqlChannelNotifier(Options()).NotifyAsync(
			"tests_reconnect",
			"after",
			CancellationToken);
		(await ReadAsync(listener, 2)).Should().Contain(x => x.Payload == "after");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public async Task Listen_RejectsEmptyChannelName(string channel)
	{
		await using var hub = CreateHub();

		await Assert.ThrowsAnyAsync<ArgumentException>(() => hub.ListenAsync(channel, CancellationToken));
	}

	[Fact]
	public async Task Listen_QuotesChannelNamesWithSpecialCharacters()
	{
		await using var hub = CreateHub();
		var channel = "tests \"quoted\"; DROP TABLE x";
		await using var listener = await hub.ListenAsync(channel, CancellationToken);

		await new NpgsqlChannelNotifier(Options()).NotifyAsync(
			channel,
			"safe",
			CancellationToken);

		(await ReadAsync(listener, 2)).Should().Contain(x => x.Payload == "safe");
	}

	private NpgsqlChannelListenerHub CreateHub()
		=> new(Options(), NullLogger<NpgsqlChannelListenerHub>.Instance);

	private IOptions<DatabaseOptions> Options()
	{
		var builder = new NpgsqlConnectionStringBuilder(Context.Database.GetConnectionString());
		return Microsoft.Extensions.Options.Options.Create(
			new DatabaseOptions
			{
				Host = builder.Host!,
				Port = builder.Port,
				Database = builder.Database!,
				Username = builder.Username!,
				Password = builder.Password!
			});
	}

	private async Task<List<ChannelNotification>> ReadAsync(
		IChannelListener listener,
		int count,
		TimeSpan? timeout = null)
	{
		var result = new List<ChannelNotification>();
		using var source = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
		source.CancelAfter(timeout ?? Timeout);
		try
		{
			await foreach (var item in listener.ReadAllAsync(source.Token))
			{
				result.Add(item);
				if (result.Count >= count) break;
			}
		}
		catch (OperationCanceledException) when (!CancellationToken.IsCancellationRequested) { }

		return result;
	}

	private async Task<long> CountListenerConnectionsAsync()
	{
		await using var connection = new NpgsqlConnection(Context.Database.GetConnectionString());
		await connection.OpenAsync(CancellationToken);
		await using var command = new NpgsqlCommand(
			"SELECT count(*) FROM pg_stat_activity WHERE application_name = @name",
			connection);
		command.Parameters.AddWithValue("name", NpgsqlChannelListenerHub.ApplicationName);
		return (long)(await command.ExecuteScalarAsync(CancellationToken))!;
	}

	private async Task TerminateListenerConnectionsAsync()
	{
		await using var connection = new NpgsqlConnection(Context.Database.GetConnectionString());
		await connection.OpenAsync(CancellationToken);
		await using var command = new NpgsqlCommand(
			"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE application_name = @name",
			connection);
		command.Parameters.AddWithValue("name", NpgsqlChannelListenerHub.ApplicationName);
		await command.ExecuteNonQueryAsync(CancellationToken);
	}

	private async Task WaitUntilAsync(Func<Task<bool>> condition)
	{
		var deadline = DateTime.UtcNow + Timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (await condition()) return;
			await Task.Delay(100, CancellationToken);
		}
	}
}
