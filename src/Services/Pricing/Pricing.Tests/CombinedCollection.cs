using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}
