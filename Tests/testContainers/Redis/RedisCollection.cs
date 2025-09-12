namespace Tests.testContainers.Redis;

[CollectionDefinition("Redis collection")]
public class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
}