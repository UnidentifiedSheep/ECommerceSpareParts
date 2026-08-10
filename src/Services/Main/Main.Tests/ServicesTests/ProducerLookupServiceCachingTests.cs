using Enums;
using FluentAssertions;
using Main.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.ServicesTests;

public sealed class ProducerLookupServiceCachingTests : IntegrationTest
{
    public ProducerLookupServiceCachingTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    [Fact]
    public async Task ProducerLookup_LoadTwiceInScope_ReturnsSameSnapshot()
    {
        var service = Scope.ServiceProvider
            .GetRequiredService<IProducerLookupService>();

        var first = await service.Load();
        var second = await service.Load();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task Load_IncludesSupplierMappings()
    {
        var producer = GetContext<ProducerTestContext>().Producers[0];
        const string supplierProducerName = "Supplier-specific producer";
        await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.Armtek)
            .WithSupplierProducerName(supplierProducerName)
            .BuildAndAddToDb(Context);

        var service = Scope.ServiceProvider
            .GetRequiredService<IProducerLookupService>();

        var lookup = await service.Load();

        lookup.ResolveId(supplierProducerName, Supplier.Armtek)
            .Should().Be(producer.Id);
    }
}
