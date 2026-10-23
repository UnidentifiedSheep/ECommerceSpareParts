using System.Text.Json;
using Abstractions;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Cronos;
using Domain.CommonEntities.Job;
using Main.Application.Lrts;
using Main.Application.Lrts.RemoveExpiredDocuments;
using Main.Application.Lrts.RunServiceTask;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Main.Migrator.DataSeeds;

public class ScheduledLrtSeed : ISeed<DContext>
{
	public int ExecutionOrder => int.MaxValue;

	public async Task SeedAsync(DContext context)
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

		var existingSchedules = await context.JobSchedules
			.AsNoTracking()
			.Where(schedule => systemNames.Contains(schedule.JobSystemName))
			.Select(schedule => new { schedule.JobSystemName, schedule.InputState })
			.ToHashSetAsync();

		var missingSchedules = defaults
			.Where(schedule => !existingSchedules.Contains(new { schedule.JobSystemName, schedule.InputState }))
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
			schedule.SetNextRunAt(CronExpression.Parse(item.Cron)
				.GetNextOccurrence(now, JobSchedule.TimeZone));
			context.JobSchedules.Add(schedule);
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
