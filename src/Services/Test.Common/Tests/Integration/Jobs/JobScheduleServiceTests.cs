using Abstractions.Models;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Domain.CommonEntities.Job;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Jobs;

public sealed class JobScheduleServiceTests(CombinedContainerFixture fixture)
	: CommonLayerIntegrationTest(fixture)
{
	private static readonly DateTimeOffset UtcNow = new(
		2026,
		8,
		21,
		10,
		2,
		30,
		TimeSpan.Zero);

	private IJobScheduleService Service => Scope.ServiceProvider.GetRequiredService<IJobScheduleService>();

	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		Scope.ServiceProvider.GetRequiredService<TestTimeProvider>().SetUtcNow(UtcNow);
	}

	[Fact]
	public async Task Create_ValidSchedule_PersistsAndCalculatesNextRun()
	{
		var id = await Service.CreateScheduleAsync(
			new NewJobScheduleDto
			{
				Name = "Test schedule",
				JobSystemName = JobScheduleTestLrt.LrtName,
				InputState = "{\"Value\":1}",
				MaxAttempts = 5,
				Cron = "*/5 * * * *",
				Enabled = true
			});

		Context.ChangeTracker.Clear();
		var schedule = await Context.JobSchedules.AsNoTracking().SingleAsync(x => x.Id == id);

		schedule.Enabled.Should().BeTrue();
		schedule.MaxAttempts.Should().Be(5);
		schedule.InputState.Should().Contain("1");
		schedule
		.NextRunAt
		.Should()
		.Be(
			new DateTime(
				2026,
				8,
				21,
				10,
				5,
				0,
				DateTimeKind.Utc));
	}

	[Fact]
	public async Task Update_CronChanged_PersistsAndRecalculatesNextRun()
	{
		var schedule = await AddScheduleAsync(true);

		await Service.UpdateScheduleAsync(
			schedule.Id,
			new PatchJobScheduleDto
			{
				Cron = PatchField<string>.From("*/5 * * * *")
			});

		Context.ChangeTracker.Clear();
		var updated = await Context.JobSchedules.AsNoTracking().SingleAsync(x => x.Id == schedule.Id);

		updated.Cron.Should().Be("*/5 * * * *");
		updated
		.NextRunAt
		.Should()
		.Be(
			new DateTime(
				2026,
				8,
				21,
				10,
				5,
				0,
				DateTimeKind.Utc));
	}

	[Fact]
	public async Task Update_Disable_PersistsDisabledState()
	{
		var schedule = await AddScheduleAsync(true);

		await Service.UpdateScheduleAsync(
			schedule.Id,
			new PatchJobScheduleDto
			{
				Enabled = PatchField<bool>.From(false)
			});

		Context.ChangeTracker.Clear();
		var updated = await Context.JobSchedules.AsNoTracking().SingleAsync(x => x.Id == schedule.Id);

		updated.Enabled.Should().BeFalse();
	}

	[Fact]
	public async Task Remove_ExistingSchedule_DeletesIt()
	{
		var schedule = await AddScheduleAsync();

		await Service.RemoveScheduleAsync(schedule.Id);

		Context.ChangeTracker.Clear();
		(await Context.JobSchedules.AsNoTracking().AnyAsync(x => x.Id == schedule.Id)).Should().BeFalse();
	}

	[Fact]
	public async Task Update_MissingSchedule_ThrowsNotFound()
	{
		var action = () => Service.UpdateScheduleAsync(Guid.NewGuid(), new PatchJobScheduleDto());

		await action.Should().ThrowAsync<JobScheduleNotFoundException>();
	}

	[Fact]
	public async Task Remove_MissingSchedule_ThrowsNotFound()
	{
		var action = () => Service.RemoveScheduleAsync(Guid.NewGuid());

		await action.Should().ThrowAsync<JobScheduleNotFoundException>();
	}

	private async Task<JobSchedule> AddScheduleAsync(bool enabled = false)
	{
		var schedule = JobSchedule.Create(
			"Test schedule",
			null,
			JobScheduleTestLrt.LrtName,
			"{\"Value\":1}",
			3,
			"0 * * * *");

		if (enabled)
			schedule.Enable();
		schedule.SetNextRunAt(UtcNow.UtcDateTime.AddHours(1));

		await Context.AddAsync(schedule);
		await Context.SaveChangesAsync();
		Context.ChangeTracker.Clear();
		return schedule;
	}
}
