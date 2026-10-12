using FluentAssertions;
using Main.Application.Handlers.ProductEans.DeleteProductEan;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEans;

public class DeleteProductEanTests : IntegrationTest
{
	public DeleteProductEanTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductEanTestContext>();
	}

	private ProductEanTestContext TestContext => GetContext<ProductEanTestContext>();

	[Fact]
	public async Task DeleteProductEan_RemovesOnlyRequestedEan()
	{
		var existing = TestContext.ProductEans.First();

		await Mediator.Send(
			new DeleteProductEanCommand(existing.ProductId, $" {existing.Ean} "),
			CancellationToken);

		var remaining = await Context.ProductEans.AsNoTracking()
			.Where(x => x.ProductId == existing.ProductId)
			.Select(x => x.Ean)
			.ToListAsync(CancellationToken);
		remaining.Should().HaveCount(2).And.NotContain(existing.Ean);
	}

	[Fact]
	public async Task DeleteProductEan_MissingEan_ThrowsNotFound()
	{
		var productId = TestContext.ProductTestContext.Products[0].Id;

		var action = () => Mediator.Send(
			new DeleteProductEanCommand(productId, "9999999999999"),
			CancellationToken);

		await action.Should().ThrowAsync<ProductEanNotFoundException>();
	}
}
