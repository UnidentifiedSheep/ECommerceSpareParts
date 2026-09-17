using System.Threading.Channels;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Tests.TestContainers.Pg;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
	private const int DatabaseCount = AssemblyFixture.MaxThreads;

	private readonly PostgreSqlContainer _postgresqlContainer =
		new PostgreSqlBuilder("postgres:latest")
			.Build();

	private Channel<PostgresDatabaseSlot> _databasePool = null!;

	public string AdminConnectionString => _postgresqlContainer.GetConnectionString();

	public async ValueTask InitializeAsync()
	{
		await _postgresqlContainer.StartAsync();

		var slots = new List<PostgresDatabaseSlot>(DatabaseCount);

		for (var i = 0; i < DatabaseCount; i++)
		{
			var databaseName = $"integration_{i}";

			await CreateDatabaseAsync(databaseName);

			slots.Add(new PostgresDatabaseSlot(
				databaseName,
				BuildConnectionString(databaseName)));
		}

		_databasePool = Channel.CreateBounded<PostgresDatabaseSlot>(
			new BoundedChannelOptions(DatabaseCount)
			{
				SingleReader = false,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.Wait
			});

		foreach (var slot in slots)
			await _databasePool.Writer.WriteAsync(slot);

		Console.WriteLine(
			$"PostgreSQL container started with {DatabaseCount} test databases");
	}

	public async ValueTask<PostgresDatabaseLease> AcquireDatabaseAsync(
		CancellationToken cancellationToken = default)
	{
		var database = await _databasePool.Reader.ReadAsync(cancellationToken);

		return new PostgresDatabaseLease(
			database,
			ReleaseDatabaseAsync);
	}

	private ValueTask ReleaseDatabaseAsync(PostgresDatabaseSlot database)
	{
		return _databasePool.Writer.WriteAsync(database);
	}

	private async Task CreateDatabaseAsync(string databaseName)
	{
		await using var connection =
			new NpgsqlConnection(AdminConnectionString);

		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = $"""
			CREATE DATABASE "{databaseName}";
			""";

		await command.ExecuteNonQueryAsync();
	}

	private string BuildConnectionString(string databaseName)
	{
		var builder = new NpgsqlConnectionStringBuilder(
			AdminConnectionString)
		{
			Database = databaseName
		};

		return builder.ConnectionString;
	}

	public async ValueTask DisposeAsync()
	{
		await _postgresqlContainer.DisposeAsync();
	}
}
