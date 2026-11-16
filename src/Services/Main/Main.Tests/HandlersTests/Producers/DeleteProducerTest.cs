using FluentAssertions;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Product.Exceptions;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Handlers.Producers;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class DeleteProducerTest : IntegrationTest
{
	public DeleteProducerTest(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task DeleteProducer_InvalidProducerId_ThrowsProducerNotFound()
	{
		var command = new DeleteProducerCommand(int.MaxValue);
		await Assert.ThrowsAsync<ProducerNotFoundException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task DeleteProducer_WhenProducerHasArticle_ThrowsCannotDeleteProducerWithArticles()
	{
		var command = new DeleteProducerCommand(TestContext.Products[0].ProducerId);
		await Assert.ThrowsAsync<CannotDeleteProducerWithArticlesException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task DeleteProducer_Normal_Succeeds()
	{
		var producer = TestContext.ProducerTestContext.Producers[0];
		var toRemove =
			await Context.Set<Product>()
				.Where(x => x.ProducerId == producer.Id)
				.ToListAsync(CancellationToken);
		if (toRemove.Count > 0)
		{
			Context.Set<Product>().RemoveRange(toRemove);
			await Context.SaveChangesAsync(CancellationToken);
		}

		var act = () => Mediator.Send(new DeleteProducerCommand(producer.Id));
		await act.Should().NotThrowAsync();

		var dbProduct = await Context.Set<Product>()
			.FirstOrDefaultAsync(
				x => x.ProducerId == producer.Id,
				CancellationToken);
		dbProduct.Should().BeNull();
	}
}
