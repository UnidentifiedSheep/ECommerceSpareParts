using Test.Common.TestContainers.Combined;

namespace Analytics.Integration.Tests;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}