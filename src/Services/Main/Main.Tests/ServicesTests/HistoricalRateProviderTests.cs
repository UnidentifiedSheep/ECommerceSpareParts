using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using FluentAssertions;
using Main.Application.Interfaces.Services.Currency;
using Main.Application.Services.Currency;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;

namespace Tests.ServicesTests;

public sealed class HistoricalRateProviderTests : IntegrationTest
{
	private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public HistoricalRateProviderTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<CurrencyRatesTestContext>();

	private CurrencyRatesTestContext Rates => GetContext<CurrencyRatesTestContext>();
	private CurrencyRate Rate => Rates.Rates.First();
	private IHistoricalRateProvider Provider => Scope.ServiceProvider.GetRequiredService<IHistoricalRateProvider>();

	[Theory]
	[InlineData(-730, 2)]
	[InlineData(0, 2)]
	[InlineData(2, 2)]
	[InlineData(5, 2)]
	[InlineData(8, 4)]
	[InlineData(10, 4)]
	[InlineData(100, 4)]
	public async Task GetNearestRate_SelectsClosestDate_AndEarlierOnTie(int days, decimal expected)
	{
		await Rates.SetHistoryAsync(Rate, [(Start.AddDays(10), 2m, 4m), (Start, 0m, 2m)], CancellationToken);

		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start.AddDays(days), CancellationToken))
			.Should().Be(expected);
	}

	[Fact]
	public async Task SameTimestamp_SelectsChangeWithLargestId()
	{
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, 2m), (Start, 2m, 4m)], CancellationToken);

		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(4m);
		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start.AddDays(-1), CancellationToken)).Should().Be(4m);
		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start.AddDays(1), CancellationToken)).Should().Be(4m);
	}

	[Fact]
	public async Task BaseCurrency_WithoutHistory_ReturnsOne()
	{
		await Rates.SetHistoryAsync(Rate, [], CancellationToken);

		(await Provider.GetNearestRateFor(Rate.ToCurrencyId, Start, CancellationToken)).Should().Be(1m);
	}

	[Fact]
	public async Task MissingCurrency_DoesNotUseAnotherCurrenciesHistory()
	{
		await Rates.SetHistoryAsync(Rate, [], CancellationToken);

		await Assert.ThrowsAsync<CurrencyRateNotFoundException>(() =>
			Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task InvalidRate_ThrowsNotFound(decimal value)
	{
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, value)], CancellationToken);

		await Assert.ThrowsAsync<CurrencyRateNotFoundException>(() =>
			Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken));
	}

	[Theory]
	[InlineData(DateTimeKind.Local)]
	[InlineData(DateTimeKind.Unspecified)]
	public async Task NonUtcDate_IsRejected(DateTimeKind kind)
	{
		await Assert.ThrowsAsync<ArgumentException>(() =>
			Provider.GetNearestRateFor(Rate.FromCurrencyId, DateTime.SpecifyKind(Start, kind), CancellationToken));
	}

	[Fact]
	public async Task Reload_ReplacesCachedSnapshot()
	{
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, 2m)], CancellationToken);
		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(2m);
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, 4m)], CancellationToken);
		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(2m);

		await Provider.Reload(CancellationToken);

		(await Provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(4m);
	}

	[Fact]
	public async Task Reload_DuringInitialLoad_DoesNotLoseNewHistory()
	{
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, 2m)], CancellationToken);
		var readCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var releaseRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var (provider, factory) = CreateProvider(async attempt =>
		{
			if (attempt != 1) return;
			readCompleted.SetResult();
			await releaseRead.Task.WaitAsync(CancellationToken);
		});
		using (provider)
		{
			var firstRead = provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken);
			Task reload;
			try
			{
				await readCompleted.Task.WaitAsync(CancellationToken);
				await Rates.SetHistoryAsync(Rate, [(Start, 0m, 4m)], CancellationToken);
				reload = provider.Reload(CancellationToken);
			}
			finally
			{
				releaseRead.TrySetResult();
			}

			(await firstRead).Should().Be(2m);
			await reload;
			(await provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(4m);
			factory.Verify(value => value.Create(), Times.Exactly(2));
		}
	}

	[Fact]
	public async Task FailedReload_PreservesPreviousSnapshot_AndCanBeRetried()
	{
		await Rates.SetHistoryAsync(Rate, [(Start, 0m, 2m)], CancellationToken);
		var (provider, _) = CreateProvider(attempt => attempt == 2
			? Task.FromException(new InvalidOperationException("Load failed")) : Task.CompletedTask);
		using (provider)
		{
			(await provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(2m);
			await Rates.SetHistoryAsync(Rate, [(Start, 0m, 4m)], CancellationToken);

			await Assert.ThrowsAsync<InvalidOperationException>(() => provider.Reload(CancellationToken));
			(await provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(2m);
			await provider.Reload(CancellationToken);
			(await provider.GetNearestRateFor(Rate.FromCurrencyId, Start, CancellationToken)).Should().Be(4m);
		}
	}

	private (HistoricalRateProvider Provider, Mock<IApplicationTransactionFactory> Factory) CreateProvider(Func<int, Task> afterExecute)
	{
		var context = new Mock<IApplicationTransactionContext>();
		context.SetupGet(value => value.Repositories).Returns(Scope.ServiceProvider.GetRequiredService<IRepositoryProvider>());
		context.Setup(value => value.GetRequiredService<ISettingsService>())
			.Returns(Scope.ServiceProvider.GetRequiredService<ISettingsService>());
		var factory = new Mock<IApplicationTransactionFactory>();
		var attempts = 0;
		factory.Setup(value => value.Create()).Returns(() =>
		{
			var attempt = Interlocked.Increment(ref attempts);
			return new ApplicationTransactionStub(context.Object, () => afterExecute(attempt));
		});
		return (new HistoricalRateProvider(factory.Object), factory);
	}
}
