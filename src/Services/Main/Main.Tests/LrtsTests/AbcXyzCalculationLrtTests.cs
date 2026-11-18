using System.Text.Json;
using Application.Common;
using Application.Common.Extensions;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using FluentValidation;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Lrts.AbcXyzCalculation;
using Main.Modules.Analytics.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchemaGeneration.Abstractions;
using SchemaGeneration.Abstractions.Enums;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.LrtsTests;

public class AbcXyzCalculationLrtTests : IntegrationTest
{
	public AbcXyzCalculationLrtTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<SaleTestContext>();

	private AbcXyzParameters Parameters()
	{
		var month = GetContext<SaleTestContext>().Sale.SaleDatetime;
		var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		return new AbcXyzParameters
		{
			From = start,
			To = start.AddMonths(1),
			AbcMetric = AnalysisMetric.Revenue,
			XyzMetric = AnalysisMetric.Quantity,
			AShare = 0.80m,
			BShare = 0.15m,
			XCoefficient = 0.10m,
			YCoefficient = 0.25m,
			MinPeriods = 2
		};
	}

	[Fact]
	public async Task Execute_SavesRunWithItemsAndLinksItToJob()
	{
		var parameters = Parameters();

		var job = await Execute(parameters);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var run = await Context.Set<AnalyticsRun>().AsNoTracking().SingleAsync(CancellationToken);
		run.JobId.Should().Be(job.Id);
		run.AnalysisType.Should().Be("AbcXyz");
		run.ParametersHash.Should().Be(AbcXyzRunFactory.ComputeHash(parameters));
		run.PeriodStart.Should().Be(parameters.From);
		run.PeriodEnd.Should().Be(parameters.EndExclusive());
		run.ItemsCount.Should().Be(1);
		var item = await Context.Set<AnalyticsRunItem>().AsNoTracking().SingleAsync(CancellationToken);
		item.RunId.Should().Be(run.Id);
		item.SubjectKey.Should().Be(GetContext<SaleTestContext>().Product.Id.ToString());
		item.Rank.Should().Be(1);
		JsonDocument.Parse(item.Segments).RootElement.GetProperty("abc").GetString().Should().Be("A");
	}

	[Fact]
	public async Task Execute_ReplacesRunPreviouslyWrittenForTheSameJob()
	{
		var parameters = Parameters();
		var stale = AnalyticsRun.Create(
			"AbcXyz",
			1,
			"{}",
			new string('0', 64),
			null,
			null,
			null,
			null);
		await Context.AddAsync(stale, CancellationToken);
		await Context.AddAsync(
			AnalyticsRunItem.Create(
				stale.Id,
				"1",
				1,
				1m,
				1m,
				null,
				null),
			CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var job = await Execute(parameters, existingRunId: stale.Id);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		(await Context.Set<AnalyticsRun>().AsNoTracking().ToListAsync(CancellationToken))
			.Should()
			.ContainSingle(run => run.Id != stale.Id);
		(await Context.Set<AnalyticsRunItem>().AnyAsync(item => item.RunId == stale.Id, CancellationToken))
			.Should()
			.BeFalse();
	}

	[Fact]
	public async Task Execute_ReadsNoSalesOutsideThePeriod()
	{
		var parameters = Parameters();
		parameters = parameters with { From = parameters.From.AddYears(-5), To = parameters.To.AddYears(-5) };

		var job = await Execute(parameters);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var run = await Context.Set<AnalyticsRun>().AsNoTracking().SingleAsync(CancellationToken);
		run.ItemsCount.Should().Be(0);
	}

	[Fact]
	public void ValidateState_RejectsInvalidParameters()
	{
		var lrt = CreateLrt();
		var json = JsonSerializer.Serialize(Parameters() with { MinPeriods = 1 });

		var act = () => lrt.ValidateState(json);

		act.Should().Throw<ValidationException>();
	}

	private AbcXyzCalculationLrt CreateLrt()
		=> ActivatorUtilities.CreateInstance<AbcXyzCalculationLrt>(Scope.ServiceProvider);

	private async Task<SingleRunJob> Execute(AbcXyzParameters parameters, Guid? existingRunId = null)
	{
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = new MessageBrokerStub();
		var lrt = CreateLrt();
		var state = lrt.ValidateState(JsonSerializer.Serialize(parameters));
		var job = SingleRunJob.Create(lrt.SystemName, state);
		var holder = Guid.NewGuid();
		job.AcquireLease(holder, TimeSpan.FromMinutes(5));
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		if (existingRunId is { } runId)
			await Context.Set<AnalyticsRun>()
				.Where(run => run.Id == runId)
				.ExecuteUpdateAsync(setters => setters.SetProperty(run => run.JobId, job.Id), CancellationToken);

		await lrt.ExecuteAsync(
			job.Id,
			holder,
			CancellationToken);
		await Context.Entry(job).ReloadAsync(CancellationToken);
		return job;
	}
}
