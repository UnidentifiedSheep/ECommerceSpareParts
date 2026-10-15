using System.Threading.Channels;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Tests.TestContainers.OpenSearch;

public sealed class OpenSearchContainerFixture : IAsyncLifetime
{
	private const int SlotCount = AssemblyFixture.MaxThreads;
	private readonly IContainer _container = new ContainerBuilder("opensearchproject/opensearch:latest")
		.WithEnvironment("discovery.type", "single-node")
		.WithEnvironment("DISABLE_SECURITY_PLUGIN", "true")
		.WithEnvironment("DISABLE_INSTALL_DEMO_CONFIG", "true")
		.WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
		.WithPortBinding(9200, true)
		.WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPort(9200).ForPath("/")))
		.Build();

	private Channel<OpenSearchIndexSlot> _indexPool = null!;

	public Uri Endpoint => new($"http://{_container.Hostname}:{_container.GetMappedPublicPort(9200)}");

	public async ValueTask InitializeAsync()
	{
		await _container.StartAsync(TestContext.Current.CancellationToken);
		_indexPool = Channel.CreateBounded<OpenSearchIndexSlot>(new BoundedChannelOptions(SlotCount)
		{
			SingleReader = false,
			SingleWriter = false,
			FullMode = BoundedChannelFullMode.Wait
		});
		var prefix = $"integration-{Guid.NewGuid():N}";
		for (var i = 0; i < SlotCount; i++)
			await _indexPool.Writer.WriteAsync(new OpenSearchIndexSlot($"{prefix}-{i}-", Endpoint));
	}

	public ValueTask DisposeAsync() => _container.DisposeAsync();

	public async ValueTask<OpenSearchIndexLease> AcquireIndicesAsync(CancellationToken cancellationToken = default)
	{
		var indices = await _indexPool.Reader.ReadAsync(cancellationToken);
		return new OpenSearchIndexLease(indices, ReleaseAsync);
	}

	private ValueTask ReleaseAsync(OpenSearchIndexSlot indices) => _indexPool.Writer.WriteAsync(indices);
}
