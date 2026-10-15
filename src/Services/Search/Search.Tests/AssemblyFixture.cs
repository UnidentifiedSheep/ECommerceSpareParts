using Tests.TestContainers.Search;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(SearchContainerFixture))]
[assembly: Parallelization(Mode = ParallelMode.Collections, MaxThreads = Tests.AssemblyFixture.MaxThreads)]
