using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Cronos;
using Domain.CommonEntities.Job;
using Main.Application.Lrts;
using Main.Application.Lrts.RemoveExpiredDocuments;
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
				new NoneInputState())
		];

		var systemNames = defaults
			.Select(x => x.JobSystemName)
			.ToList();

		var existingSystemNames = await context.JobSchedules
			.AsNoTracking()
			.Where(schedule => systemNames.Contains(schedule.JobSystemName))
			.Select(schedule => schedule.JobSystemName)
			.ToHashSetAsync();

		var missingSchedules = defaults
			.Where(schedule => !existingSystemNames.Contains(schedule.JobSystemName))
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
