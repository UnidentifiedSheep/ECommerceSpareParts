using Domain.CommonEnums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Lrts.ResyncOurWarehouseOffers;
using Pricing.Entities.Enums;
using Pricing.Entities.Offers;
using Tests.Extensions;
using Tests.PricingTests.DataBuilders.Offers;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class ResyncOurWarehouseOffersLrtTests(CombinedContainerFixture fixture)
	: LrtIntegrationTest<ResyncOurWarehouseOffersLrt>(fixture)
{
	[Fact]
	public async Task Execute_RemovesEmptyOurWarehouseOffersAndKeepsSupplierOnes()
	{
		await AddOfferAsync(PriceOfferSource.OurWarehouse, 0);
		await AddOfferAsync(PriceOfferSource.OurWarehouse, -2);
		var supplierEmpty = await AddOfferAsync(PriceOfferSource.Armtek, 0);

		var execution = await ExecuteLrt(cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var remaining = await Context.Set<PriceOffer>()
			.AsNoTracking()
			.Select(x => x.Id)
			.ToListAsync(CancellationToken);
		remaining.Should().BeEquivalentTo([supplierEmpty.Id]);
	}

	private Task<PriceOffer> AddOfferAsync(PriceOfferSource source, int quantity)
		=> new PriceOfferDataBuilder(Faker)
			.WithSource(source)
			.WithAvailableQuantity(quantity)
			.BuildAndAddToDb(Context, CancellationToken);
}
