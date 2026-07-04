using FluentAssertions;
using Main.Application.Handlers.Producers;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class DeleteProducerTest : IntegrationTest
{
    public DeleteProducerTest(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task DeleteProducer_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteProducerCommand(int.MaxValue);
        await Assert.ThrowsAsync<ProducerNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteProducer_WhenProducerHasArticle_ThrowsCannotDeleteProducerWithArticles()
    {
        var command = new DeleteProducerCommand(TestContext.Products[0].ProducerId);
        await Assert.ThrowsAsync<CannotDeleteProducerWithArticlesException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteProducer_Normal_Succeeds()
    {
        var producer = TestContext.ProducerTestContext.Producers[0];
        var toRemove = await Context.Products.Where(x => x.ProducerId == producer.Id).ToListAsync();
        if (toRemove.Count > 0)
        {
            Context.Products.RemoveRange(toRemove);
            await Context.SaveChangesAsync();
        }

        var act = () => Mediator.Send(new DeleteProducerCommand(producer.Id));
        await act.Should().NotThrowAsync();

        var dbProduct = await Context.Products.FirstOrDefaultAsync(x => x.ProducerId == producer.Id);
        dbProduct.Should().BeNull();
    }
}