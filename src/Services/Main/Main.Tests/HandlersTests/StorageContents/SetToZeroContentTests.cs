using FluentAssertions;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class SetToZeroContentTests : IntegrationTest
{
    public SetToZeroContentTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
    }

    private StorageContentTestContext TestContext => GetContext<StorageContentTestContext>();

    [Fact]
    public async Task SetToZeroContent_WithInvalidContentId_ThrowsStorageContentNotFoundException()
    {
        var command = new SetToZeroContentCommand(99999, 0);
        await Assert.ThrowsAsync<StorageContentNotFoundException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task SetToZeroContent_WithValidData_Succeeds()
    {
        var eventsCount = await Context.Events.CountAsync();
        var content = TestContext.StorageContents.First();
        var contentCount = content.Count;
        var productCountBefore = (await Context.Products
                .FirstAsync(x => x.Id == content.ProductId))
            .Stock;

        var command = new SetToZeroContentCommand(content.Id, content.RowVersion);

        await Mediator.Send(command);

        var productCountAfter = (await Context.Products
                .FirstAsync(x => x.Id == content.ProductId))
            .Stock;

        var currStorageMovements = await Context.Events.CountAsync();

        currStorageMovements.Should().Be(eventsCount + 1);
        productCountBefore.Value.Should().Be(productCountAfter + contentCount);
    }
}