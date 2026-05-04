using FluentAssertions;
using Main.Application.Handlers.Producers.DeleteProducer;
using Main.Entities.Exceptions.Producers;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Base;

namespace Tests.HandlersTests.Producers;

public class DeleteProducerTest : TestBase
{
    public DeleteProducerTest(CombinedContainerFixture fixture) : base(fixture)
    {
        ProductTestContext.Register(this);
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
        await Assert.ThrowsAsync<CannotDeleteProducerWithArticlesException>(async () => await Mediator.Send(command));
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