using Abstractions.Interfaces.Persistence;
using Abstractions.Models;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Services.Job;
using Application.Common.Validators;
using Domain.CommonEntities.Job;
using FluentAssertions;
using Moq;
using Tests.Stubs;

namespace Tests.Tests.Services.Job;

public sealed class JobScheduleServiceTests
{
    private static readonly DateTimeOffset UtcNow =
        new(2026, 8, 21, 10, 2, 30, TimeSpan.Zero);

    [Fact]
    public async Task Create_ValidSchedule_CreatesAndCalculatesNextRun()
    {
        var fixture = new Fixture();
        JobSchedule? added = null;
        fixture.UnitOfWork
            .Setup(x => x.AddAsync(
                It.IsAny<JobSchedule>(),
                It.IsAny<CancellationToken>()))
            .Callback<JobSchedule, CancellationToken>((schedule, _) => added = schedule)
            .Returns(Task.CompletedTask);

        await fixture.Service.CreateScheduleAsync(new NewJobScheduleDto
        {
            Name = "Test schedule",
            JobSystemName = Fixture.LrtSystemName,
            InputState = "{\"Value\":1}",
            MaxAttempts = 5,
            Cron = "*/5 * * * *",
            Enabled = true
        });

        added.Should().NotBeNull();
        added!.Enabled.Should().BeTrue();
        added.NextRunAt.Should().Be(new DateTime(2026, 8, 21, 10, 5, 0, DateTimeKind.Utc));
        added.InputState.Should().Contain("1");
        fixture.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_CronChanged_RecalculatesNextRunAndLocksSchedule()
    {
        var fixture = new Fixture();
        var schedule = CreateSchedule(enabled: true);
        Criteria<JobSchedule>? criteria = null;
        fixture.Repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<JobSchedule>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Criteria<JobSchedule>?, CancellationToken>((value, _) => criteria = value)
            .ReturnsAsync(schedule);

        await fixture.Service.UpdateScheduleAsync(
            Guid.NewGuid(),
            new PatchJobScheduleDto
            {
                Cron = PatchField<string>.From("*/5 * * * *")
            });

        schedule.Cron.Should().Be("*/5 * * * *");
        schedule.NextRunAt.Should().Be(new DateTime(2026, 8, 21, 10, 5, 0, DateTimeKind.Utc));
        criteria.Should().NotBeNull();
        criteria!.Track.Should().BeTrue();
        criteria.ForUpdate.Should().BeTrue();
    }

    [Fact]
    public async Task Update_DisabledSchedule_DisablesWithoutRecalculatingNextRun()
    {
        var fixture = new Fixture();
        var schedule = CreateSchedule(enabled: true);
        var existingNextRun = new DateTime(2026, 8, 22, 10, 0, 0, DateTimeKind.Utc);
        schedule.SetNextRunAt(existingNextRun);
        fixture.Repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<JobSchedule>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        await fixture.Service.UpdateScheduleAsync(
            Guid.NewGuid(),
            new PatchJobScheduleDto
            {
                Enabled = PatchField<bool>.From(false)
            });

        schedule.Enabled.Should().BeFalse();
        schedule.NextRunAt.Should().Be(existingNextRun);
    }

    [Fact]
    public async Task Remove_ExistingSchedule_RemovesLockedSchedule()
    {
        var fixture = new Fixture();
        var schedule = CreateSchedule();
        Criteria<JobSchedule>? criteria = null;
        fixture.Repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<JobSchedule>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Criteria<JobSchedule>?, CancellationToken>((value, _) => criteria = value)
            .ReturnsAsync(schedule);

        await fixture.Service.RemoveScheduleAsync(Guid.NewGuid());

        fixture.UnitOfWork.Verify(x => x.Remove(schedule), Times.Once);
        criteria.Should().NotBeNull();
        criteria!.Track.Should().BeTrue();
        criteria.ForUpdate.Should().BeTrue();
    }

    [Fact]
    public async Task Update_MissingSchedule_ThrowsNotFound()
    {
        var fixture = new Fixture();
        fixture.Repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<JobSchedule>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobSchedule?)null);

        var action = () => fixture.Service.UpdateScheduleAsync(
            Guid.NewGuid(),
            new PatchJobScheduleDto());

        await action.Should().ThrowAsync<JobScheduleNotFoundException>();
    }

    [Fact]
    public async Task Remove_MissingSchedule_ThrowsNotFound()
    {
        var fixture = new Fixture();
        fixture.Repository
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<Criteria<JobSchedule>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobSchedule?)null);

        var action = () => fixture.Service.RemoveScheduleAsync(Guid.NewGuid());

        await action.Should().ThrowAsync<JobScheduleNotFoundException>();
    }

    private static JobSchedule CreateSchedule(bool enabled = false)
    {
        var schedule = JobSchedule.Create(
            "Test schedule",
            null,
            Fixture.LrtSystemName,
            "{\"Value\":1}",
            3,
            "0 * * * *");

        if (enabled) schedule.Enable();
        return schedule;
    }

    private sealed class Fixture
    {
        public const string LrtSystemName = "test-lrt";

        public Fixture()
        {
            UnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Repositories
                .Setup(x => x.Get<JobSchedule, Guid>())
                .Returns(Repository.Object);

            var lrt = new Mock<ILrtNamedObject>();
            lrt.SetupGet(x => x.SystemName).Returns(LrtSystemName);
            lrt.SetupGet(x => x.InputType).Returns(typeof(TestInputState));
            lrt.SetupGet(x => x.StateType).Returns(typeof(TestInputState));

            Registry
                .Setup(x => x.GetBySystemName(LrtSystemName))
                .Returns(lrt.Object);

            TimeProvider.Setup(x => x.GetUtcNow()).Returns(UtcNow);

            Service = new JobScheduleService(
                new ApplicationTransactionServiceStub(
                    UnitOfWork.Object,
                    Repositories.Object),
                Registry.Object,
                Mock.Of<IJobService>(),
                new NewJobScheduleDtoValidator(),
                new PatchJobScheduleDtoValidator(),
                TimeProvider.Object);
        }

        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<IRepositoryProvider> Repositories { get; } = new();
        public Mock<IRepository<JobSchedule, Guid>> Repository { get; } = new();
        public Mock<INamedObjectRegistry<ILrtNamedObject>> Registry { get; } = new();
        public Mock<TimeProvider> TimeProvider { get; } = new();
        public JobScheduleService Service { get; }
    }

    private sealed class TestInputState : IInputState
    {
        public int Value { get; init; }
        public void ValidateState() { }
    }
}
