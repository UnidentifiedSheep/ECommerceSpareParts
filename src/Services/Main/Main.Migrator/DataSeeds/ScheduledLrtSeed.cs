using System.Text.Json;
using Abstractions;
using Application.Common;
using Application.Common.Interfaces;
using Cronos;
using Domain.CommonEntities.Job;
using Main.Modules.Analytics.Application.Lrts.RemoveOldAnalyticsRuns;
using Main.Modules.Currency.Application.Lrts;
using Main.Modules.Documents.Application.Lrts.RemoveExpiredDocuments;
using Main.Modules.Identity.Application.Lrts;
using Main.Modules.Jobs.Application.LRT.RunServiceTask;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class ScheduledLrtSeed : ISeed<UnifiedDbContext>
{
	public int ExecutionOrder => int.MaxValue;

	public async Task SeedAsync(UnifiedDbContext context)
	{
		DefaultSchedule[] defaults =
		[
			DefaultSchedule.Create(
				"Update currency rates",
				"Updates exchange rates once a day.",
				UpdateCurrencyRatesLrt.Name,
				"0 3 * * *",
				new NoneInputState()),
			DefaultSchedule.Create(
				"Remove expired documents",
				"Removes expired generated documents once a day.",
				RemoveExpiredDocumentsLrt.Name,
				"0 4 * * *",
				new NoneInputState()),
			DefaultSchedule.Create(
				"Remove expired user tokens",
				"Removes expired user tokens once a week.",
				ClearExpiredTokensLrt.Name,
				"0 3 * * 0",
				new NoneInputState()),
			DefaultSchedule.Create(
				"Remove old analytics runs",
				"Removes unpinned analytics runs older than 90 days once a month.",
				RemoveOldAnalyticsRunsLrt.Name,
				"0 5 1 * *",
				new RemoveOldAnalyticsRunsInputState
				{
					OlderThanDays = 90
				}),
			DefaultSchedule.Create(
				"Remove deleted search products",
				"Removes deleted product documents from the search index once a day.",
				RunServiceTaskLrt.Name,
				"0 0 * * *",
				new RunServiceTaskInputState
				{
					ServiceName = ServicesDefinitions.Search.ServiceName,
					TaskSystemName = "RemoveDeletedProductsLrt"
				})
		];

		var systemNames = defaults
			.Select(x => x.JobSystemName)
			.ToList();

		var existingSchedules = await context.Set<JobSchedule>()
			.AsNoTracking()
			.Where(schedule => systemNames.Contains(schedule.JobSystemName))
			.Select(schedule => new { schedule.JobSystemName, schedule.InputState })
			.ToHashSetAsync();

		var missingSchedules = defaults
			.Where(schedule
				=> !existingSchedules.Contains(new { schedule.JobSystemName, schedule.InputState }))
			.ToArray();

		if (missingSchedules.Length == 0) return;

		var now = DateTime.UtcNow;
		foreach (var item in missingSchedules)
		{
			var schedule = JobSchedule.Create(
				item.Name,
				item.Description,
				item.JobSystemName,
				item.InputState,
				3,
				item.Cron);
			schedule.Enable();
			schedule.SetNextRunAt(
				CronExpression.Parse(item.Cron)
					.GetNextOccurrence(now, JobSchedule.TimeZone));
			context.Set<JobSchedule>().Add(schedule);
		}

		await context.SaveChangesAsync();
	}

	private sealed record DefaultSchedule(
		string Name,
		string Description,
		string JobSystemName,
		string Cron,
		string InputState)
	{
		public static DefaultSchedule Create<TInputState>(
			string name,
			string description,
			string jobSystemName,
			string cron,
			TInputState inputState) where TInputState : class, IInputState
		{
			inputState.ValidateState();
			return new DefaultSchedule(
				name,
				description,
				jobSystemName,
				cron,
				JsonSerializer.Serialize(inputState));
		}
	}
}
