namespace Tests.TestContainers.Combined;

public sealed record TestEnvironmentSlot(
	int Index,
	string PostgresConnectionString,
	string RedisConnectionString);
