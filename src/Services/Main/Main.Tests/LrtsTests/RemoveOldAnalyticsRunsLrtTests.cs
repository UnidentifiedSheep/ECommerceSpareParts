using System.Text.Json;
using Application.Common;
using Application.Common.Extensions;
using Application.Common.Interfaces.Persistence;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using FluentValidation;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Abstractions.Models;
using Main.Modules.Analytics.Application.Lrts.RemoveOldAnalyticsRuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchemaGeneration.Abstractions;
using Tests.AnalyticsTests;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public class RemoveOldAnalyticsRunsLrtTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public async Task Execute_RemovesOldUnpinnedRunsWithItemsAndKeepsPinnedAndRecentOnes()
	{
		var old = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 120);
		var oldPinned = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 200, pinned: true);
		var recent = await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 10);
		foreach (var run in new[] { old, oldPinned, recent })
			await AnalyticsRunTestData.AddItemsAsync(
				Context,
				run.Id,
				CancellationToken,
				(1, 1, AbcClass.A, XyzClass.X));

		var job = await Execute(new RemoveOldAnalyticsRunsInputState { OlderThanDays = 90 });

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var remaining = await Context.Set<AnalyticsRun>().AsNoTracking().Select(run => run.Id).ToListAsync(CancellationToken);
		remaining.Should().BeEquivalentTo([oldPinned.Id, recent.Id]);
		var items = await Context.Set<AnalyticsRunItem>().AsNoTracking().Select(item => item.RunId).ToListAsync(CancellationToken);
		items.Should().BeEquivalentTo([oldPinned.Id, recent.Id]);
	}

	[Fact]
	public async Task Execute_RemovesMoreRunsThanOneBatch()
	{
		for (var i = 0; i < 3; i++)
			await AnalyticsRunTestData.AddRunAsync(Context, CancellationToken, ageInDays: 100 + i);

		var job = await Execute(new RemoveOldAnalyticsRunsInputState { OlderThanDays = 90 }, batchSize: 2);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		(await Context.Set<AnalyticsRun>().AnyAsync(CancellationToken)).Should().BeFalse();
	}

	[Fact]
	public void ValidateState_RejectsNonPositiveDays()
	{
		var lrt = CreateLrt();

		var act = () => lrt.ValidateState(JsonSerializer.Serialize(new { olderThanDays = 0 }));

		act.Should().Throw<ValidationException>();
	}

	[Fact]
	public void InputSchema_HasLocalizedRequiredField()
	{
		var generator = Scope.ServiceProvider.GetRequiredService<ISchemaGenerator>();

		var schema = generator.Generate<RemoveOldAnalyticsRunsInputState>();

		var field = schema.Fields.Should().ContainSingle().Subject;
		field.Name.Should().Be("olderThanDays");
		field.Required.Should().BeTrue();
		field.Label.Should().NotStartWith("remove.old.");
		field.Description.Should().NotStartWith("remove.old.");
	}

	private RemoveOldAnalyticsRunsLrt CreateLrt(int? batchSize = null)
		=> batchSize is { } size
			? new SmallBatchLrt(
				Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>(),
				Scope.ServiceProvider.GetRequiredService<ILogger<RemoveOldAnalyticsRunsLrt>>(),
				size)
			: ActivatorUtilities.CreateInstance<RemoveOldAnalyticsRunsLrt>(Scope.ServiceProvider);

	private async Task<SingleRunJob> Execute(RemoveOldAnalyticsRunsInputState input, int? batchSize = null)
	{
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = new MessageBrokerStub();
		var lrt = CreateLrt(batchSize);
		var state = lrt.ValidateState(JsonSerializer.Serialize(input));
		var job = SingleRunJob.Create(lrt.SystemName, state);
		var holder = Guid.NewGuid();
		job.AcquireLease(holder, TimeSpan.FromMinutes(5));
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		await lrt.ExecuteAsync(job.Id, holder, CancellationToken);
		await Context.Entry(job).ReloadAsync(CancellationToken);
		return job;
	}

	private sealed class SmallBatchLrt(
		IApplicationTransactionFactory transactionService,
		ILogger<RemoveOldAnalyticsRunsLrt> logger,
		int batchSize) : RemoveOldAnalyticsRunsLrt(transactionService, logger)
	{
		protected override int BatchSize => batchSize;
	}
}
