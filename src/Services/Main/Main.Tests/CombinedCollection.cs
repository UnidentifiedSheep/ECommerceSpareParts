using Test.Common.TestContainers.Combined;
using Xunit;

namespace Test.Common.TestContainers;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}