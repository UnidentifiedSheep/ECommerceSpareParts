namespace Tests.testContainers.Pg;

[CollectionDefinition("Postgres collection")]
public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}
