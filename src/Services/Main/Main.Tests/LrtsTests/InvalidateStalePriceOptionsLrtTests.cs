using Application.Common.Interfaces.Settings;
using Domain.CommonEnums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application.Interfaces.Markup;
using Pricing.Application.Interfaces.Pricing.PriceApplier;
using Pricing.Application.Lrts.InvalidateStalePriceOptions;
using Pricing.Entities.Offers;
using Pricing.Entities.Settings;
using Tests.Extensions;
using Tests.PricingTests.DataBuilders.Offers;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class InvalidateStalePriceOptionsLrtTests(CombinedContainerFixture fixture)
	: LrtIntegrationTest<InvalidateStalePriceOptionsLrt>(fixture)
{
	[Fact]
	public async Task Execute_RemovesStaleAndOutOfStockOptionsAndKeepsActual()
	{
		var markupVersion = Scope.ServiceProvider.GetRequiredService<IMarkupContainer>().CurrentVersion;
		var appliersVersion = await Scope.ServiceProvider.GetRequiredService<IPriceApplierService>()
			.GetCurrentConfigurationVersionAsync(CancellationToken);
		var settingsVersion = (await Scope.ServiceProvider.GetRequiredService<ISettingsService>()
			.GetOrDefault<PricingSetting>(CancellationToken)).Data.Version;

		var actual = await AddOptionAsync(5, markupVersion, appliersVersion, settingsVersion);
		await AddOptionAsync(5, "outdated", appliersVersion, settingsVersion);
		await AddOptionAsync(0, markupVersion, appliersVersion, settingsVersion);
		await AddOptionAsync(-3, markupVersion, appliersVersion, settingsVersion);

		var execution = await ExecuteLrt(cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var remaining = await Context.Set<ProductPriceOption>()
			.AsNoTracking()
			.Select(x => x.PriceOfferId)
			.ToListAsync(CancellationToken);
		remaining.Should().BeEquivalentTo([actual.PriceOfferId]);
	}

	private async Task<ProductPriceOption> AddOptionAsync(
		int quantity,
		string markupVersion,
		string appliersVersion,
		Guid settingsVersion)
	{
		var offer = await new PriceOfferDataBuilder(Faker)
			.WithAvailableQuantity(quantity)
			.BuildAndAddToDb(Context, CancellationToken);
		return await new ProductPriceOptionDataBuilder(Faker)
			.WithPriceOfferId(offer.Id)
			.WithVersions(
				markupVersion,
				appliersVersion,
				settingsVersion)
			.BuildAndAddToDb(Context, CancellationToken);
	}
}
