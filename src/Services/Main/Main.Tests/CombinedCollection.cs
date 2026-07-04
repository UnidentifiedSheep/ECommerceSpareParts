using Tests.TestContainers.Combined;

namespace Tests;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}