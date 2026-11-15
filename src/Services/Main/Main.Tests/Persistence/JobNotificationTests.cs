using Domain.CommonEntities.Job;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tests.TestContainers.Combined;

namespace Tests.Persistence;

public class JobNotificationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task InsertPendingJob_NotifiesJobReadyChannel()
	{
		await using var listener = await OpenListenerAsync();

		var job = SingleRunJob.Create("NotificationTest", "{}", 1);
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		(await WaitForNotificationAsync(listener, TimeSpan.FromSeconds(10)))
			.Should()
			.Be(job.Id.ToString());
	}

	[Fact]
	public async Task LeasingJob_DoesNotNotify()
	{
		var job = SingleRunJob.Create("NotificationTest", "{}", 1);
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await using var listener = await OpenListenerAsync();
		job.AcquireLease(Guid.NewGuid(), TimeSpan.FromMinutes(5));
		await Context.SaveChangesAsync(CancellationToken);

		(await WaitForNotificationAsync(listener, TimeSpan.FromSeconds(2))).Should().BeNull();
	}

	private async Task<NpgsqlConnection> OpenListenerAsync()
	{
		var connection = new NpgsqlConnection(Context.Database.GetConnectionString());
		await connection.OpenAsync(CancellationToken);
		await using var command = new NpgsqlCommand("LISTEN job_ready", connection);
		await command.ExecuteNonQueryAsync(CancellationToken);
		return connection;
	}

	private async Task<string?> WaitForNotificationAsync(NpgsqlConnection listener, TimeSpan timeout)
	{
		string? payload = null;
		listener.Notification += (_, args) => payload = args.Payload;
		using var source = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
		source.CancelAfter(timeout);
		try
		{
			await listener.WaitAsync(source.Token);
		}
		catch (OperationCanceledException) when (!CancellationToken.IsCancellationRequested) { }

		return payload;
	}
}
