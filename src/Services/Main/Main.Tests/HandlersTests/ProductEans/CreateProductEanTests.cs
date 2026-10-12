using FluentAssertions;
using Main.Application.Handlers.ProductEans.CreateProductEan;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class CreateProductEanTests : IntegrationTest
{
	public CreateProductEanTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductEanTestContext>();
	}

	private ProductEanTestContext TestContext => GetContext<ProductEanTestContext>();

	[Fact]
	public async Task CreateProductEan_AddsEanToProduct()
	{
		var productId = TestContext.ProductTestContext.Products[0].Id;
		const string ean = " 4601234567890 ";

		await Mediator.Send(new CreateProductEanCommand(productId, ean), CancellationToken);

		var saved = await Context.ProductEans.AsNoTracking()
			.SingleAsync(x => x.ProductId == productId && x.Ean == ean.Trim(), CancellationToken);
		saved.Ean.Should().Be(ean.Trim());
	}

	[Fact]
	public async Task CreateProductEan_ExistingEan_ThrowsConflict()
	{
		var existing = TestContext.ProductEans.First();

		var action = () => Mediator.Send(
			new CreateProductEanCommand(existing.ProductId, $" {existing.Ean} "),
			CancellationToken);

		await action.Should().ThrowAsync<ProductEanAlreadyExistsException>();
	}
}
