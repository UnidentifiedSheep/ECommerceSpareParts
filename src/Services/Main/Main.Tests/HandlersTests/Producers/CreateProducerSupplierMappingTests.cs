using Enums;
using FluentAssertions;
using Main.Application.Dtos.Producer.SupplierMappings;
using Main.Application.Handlers.ProducerSupplierMappings.CreateProducerSupplierMapping;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class CreateProducerSupplierMappingTests : IntegrationTest
{
    public CreateProducerSupplierMappingTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CreateProducerSupplierMapping_InvalidSupplierProducerName_ThrowsValidationException(
        string supplierProducerName)
    {
        var command = new CreateProducerSupplierMappingCommand(
            new NewProducerSupplierMapping
            {
                ProducerId = TestContext.Producers[0].Id,
                Supplier = Supplier.Armtek,
                SupplierProducerName = supplierProducerName
            });

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateProducerSupplierMapping_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new CreateProducerSupplierMappingCommand(
            new NewProducerSupplierMapping
            {
                ProducerId = int.MaxValue,
                Supplier = Supplier.Armtek,
                SupplierProducerName = Faker.Lorem.Letter(20)
            });

        var exception = await Assert.ThrowsAsync<DbValidationException>(async () =>
            await Mediator.Send(command));

        Assert.Equal(ApplicationErrors.ProducersNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateProducerSupplierMapping_WithSameProducerAndSupplier_ThrowsMappingAlreadyExists()
    {
        var existing = await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(TestContext.Producers[0].Id)
            .WithSupplier(Supplier.Armtek)
            .BuildAndAddToDb(Context);

        var command = new CreateProducerSupplierMappingCommand(
            new NewProducerSupplierMapping
            {
                ProducerId = existing.ProducerId,
                Supplier = existing.Supplier,
                SupplierProducerName = Faker.Lorem.Letter(20)
            });

        await Assert.ThrowsAsync<ProducersSupplierMappingAlreadyExistsException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task CreateProducerSupplierMapping_Normal_Succeeds()
    {
        var producer = TestContext.Producers[0];
        var supplierProducerName = $"  {Faker.Lorem.Letter(20)}  ";
        var command = new CreateProducerSupplierMappingCommand(
            new NewProducerSupplierMapping
            {
                ProducerId = producer.Id,
                Supplier = Supplier.Armtek,
                SupplierProducerName = supplierProducerName
            });

        var result = await Mediator.Send(command);

        result.ProducerSupplierMapping.ProducerId.Should().Be(producer.Id);
        result.ProducerSupplierMapping.Supplier.Should().Be(command.ProducerSupplierMapping.Supplier);
        result.ProducerSupplierMapping.SupplierProducerName.Should().Be(supplierProducerName.Trim());

        var mapping = await Context.ProducerSupplierMappings
            .AsNoTracking()
            .SingleAsync();

        mapping.Id.Should().Be(result.ProducerSupplierMapping.Id);
        mapping.ProducerId.Should().Be(producer.Id);
        mapping.Supplier.Should().Be(command.ProducerSupplierMapping.Supplier);
        mapping.SupplierProducerName.Should().Be(supplierProducerName.Trim());
    }
}
