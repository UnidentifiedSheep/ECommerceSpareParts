using Test.Common.TestContainers.Combined;
using Xunit;

namespace Test.Common;

[CollectionDefinition("Combined collection")]
public class CombinedCollection : ICollectionFixture<CombinedContainerFixture>
{
}