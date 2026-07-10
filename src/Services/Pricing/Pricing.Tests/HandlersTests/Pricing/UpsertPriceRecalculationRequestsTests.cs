using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Dtos.Price;
using Pricing.Application.Handlers.Pricing;
using Tests.TestContainers.Combined;

namespace Pricing.Integration.Tests.HandlersTests.Pricing;

public class UpsertPriceRecalculationRequestsTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task UpsertPriceRecalculationRequests_WithValidData_CreatesRequests()
    {
        var command = new UpsertPriceRecalculationRequestsCommand(
        [
            new PriceRecalculationRequestDto
            {
                ProductId = 1,
                StorageName = "Storage A"
            },
            new PriceRecalculationRequestDto
            {
                ProductId = 2,
                StorageName = "Storage B"
            }
        ]);

        await Mediator.Send(command);

        var requests = await Context.PriceRecalculationRequests
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ToListAsync();

        requests.Should().HaveCount(2);
        requests[0].ProductId.Should().Be(1);
        requests[0].StorageName.Should().Be("Storage A");
        requests[0].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        requests[1].ProductId.Should().Be(2);
        requests[1].StorageName.Should().Be("Storage B");
        requests[1].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpsertPriceRecalculationRequests_WithDuplicateData_CreatesSingleRequest()
    {
        var command = new UpsertPriceRecalculationRequestsCommand(
        [
            new PriceRecalculationRequestDto
            {
                ProductId = 1,
                StorageName = "Storage A"
            },
            new PriceRecalculationRequestDto
            {
                ProductId = 1,
                StorageName = "Storage A"
            }
        ]);

        await Mediator.Send(command);

        var requests = await Context.PriceRecalculationRequests
            .AsNoTracking()
            .ToListAsync();

        requests.Should().ContainSingle();
        requests[0].ProductId.Should().Be(1);
        requests[0].StorageName.Should().Be("Storage A");
    }

    [Fact]
    public async Task UpsertPriceRecalculationRequests_WithExistingRequest_UpdatesUpdatedAt()
    {
        var command = new UpsertPriceRecalculationRequestsCommand(
        [
            new PriceRecalculationRequestDto
            {
                ProductId = 1,
                StorageName = "Storage A"
            }
        ]);

        await Mediator.Send(command);

        var created = await Context.PriceRecalculationRequests
            .AsNoTracking()
            .SingleAsync();

        await Task.Delay(20);
        await Mediator.Send(command);

        var updated = await Context.PriceRecalculationRequests
            .AsNoTracking()
            .SingleAsync();

        updated.ProductId.Should().Be(created.ProductId);
        updated.StorageName.Should().Be(created.StorageName);
        updated.UpdatedAt.Should().BeAfter(created.UpdatedAt);
    }
}
