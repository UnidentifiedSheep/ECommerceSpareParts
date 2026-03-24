using Test.Common.TestContainers.Combined;

namespace Tests;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}