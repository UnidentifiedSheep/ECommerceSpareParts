using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Search.Abstractions.Options;
using Search.Application.Interfaces.Product;
using Search.Application.Lrts;

namespace Search.Tests.Lrts;

public sealed class RemoveDeletedProductsLrtUnitTests
{
	[Fact]
	public async Task Execute_UsesOneUtcCutoffAndContinuesUntilThereAreNoMoreDeletedDocuments()
	{
		var now = new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
		var clock = new Mock<TimeProvider>();
		clock.SetupSequence(value => value.GetUtcNow()).Returns(now).Returns(now.AddHours(1));
		var repository = new Mock<IProductRepository>();
		var token = TestContext.Current.CancellationToken;
		repository.SetupSequence(value => value.DeleteDeletedBefore(It.IsAny<DateTime>(), 2, token))
			.ReturnsAsync(2).ReturnsAsync(1).ReturnsAsync(0);
		var lrt = Create(repository.Object, clock.Object);

		await lrt.ExecuteAsync(Guid.Empty, Guid.Empty, token);

		repository.Verify(value => value.DeleteDeletedBefore(now.UtcDateTime.AddDays(-7), 2, token), Times.Exactly(3));
		clock.Verify(value => value.GetUtcNow(), Times.Once);
	}

	[Fact]
	public async Task Execute_PropagatesRepositoryFailureToTheConsumer()
	{
		var repository = new Mock<IProductRepository>();
		var failure = new InvalidOperationException("OpenSearch failed.");
		repository.Setup(value => value.DeleteDeletedBefore(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(failure);

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
			Create(repository.Object).ExecuteAsync(Guid.Empty, Guid.Empty, TestContext.Current.CancellationToken));

		exception.Should().BeSameAs(failure);
	}

	[Fact]
	public async Task Execute_StopsBetweenBatchesWhenCancelled()
	{
		using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
		var repository = new Mock<IProductRepository>();
		repository.Setup(value => value.DeleteDeletedBefore(It.IsAny<DateTime>(), It.IsAny<int>(), cancellation.Token))
			.Callback(() => cancellation.Cancel()).ReturnsAsync(1);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			Create(repository.Object).ExecuteAsync(Guid.Empty, Guid.Empty, cancellation.Token));

		repository.Verify(value => value.DeleteDeletedBefore(It.IsAny<DateTime>(), It.IsAny<int>(), cancellation.Token), Times.Once);
	}

	private static RemoveDeletedProductsLrt Create(IProductRepository repository, TimeProvider? clock = null)
	{
		var options = Mock.Of<IOptionsMonitor<DeletedProductsCleanupOptions>>(value =>
			value.CurrentValue == new DeletedProductsCleanupOptions { RetentionDays = 7, BatchSize = 2 });
		return new RemoveDeletedProductsLrt(repository, options, clock ?? TimeProvider.System, NullLogger<RemoveDeletedProductsLrt>.Instance);
	}
}
