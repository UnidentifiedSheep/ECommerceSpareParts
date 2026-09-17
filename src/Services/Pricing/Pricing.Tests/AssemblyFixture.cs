using Tests;
using Tests.TestContainers.Combined;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyFixture(typeof(CombinedContainerFixture))]
[assembly: Parallelization(Mode = ParallelMode.Collections, MaxThreads = AssemblyFixture.MaxThreads)]
