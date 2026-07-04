using Domain.CommonEntities;
using Exceptions;
using FluentAssertions;

namespace Test.Common.Tests.CommonEntities;

public class JobScheduleTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var schedule = JobSchedule.Create(
            "  Daily import  ",
            "  Imports products  ",
            "import-products",
            "  {\"page\":1}  ",
            5,
            "  0 0 * * *  ");

        schedule.Name.Should().Be("Daily import");
        schedule.Description.Should().Be("Imports products");
        schedule.JobSystemName.Should().Be("import-products");
        schedule.InputState.Should().Be("{\"page\":1}");
        schedule.MaxAttempts.Should().Be(5);
        schedule.Cron.Should().Be("0 0 * * *");
        schedule.Enabled.Should().BeFalse();
        schedule.LastQueuedAt.Should().BeNull();
        schedule.NextRunAt.Should().BeNull();
        schedule.Runs.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithoutDescription_Succeeds()
    {
        var schedule = JobSchedule.Create(
            "Daily import",
            null,
            "import-products",
            "{}",
            3,
            "0 0 * * *");

        schedule.Description.Should().BeNull();
    }

    [Fact]
    public void Create_BlankInputState_Throws()
    {
        var act = () => JobSchedule.Create(
            "Daily import",
            null,
            "import-products",
            "   ",
            3,
            "0 0 * * *");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void Create_InvalidMaxAttempts_Throws()
    {
        var act = () => JobSchedule.Create(
            "Daily import",
            null,
            "import-products",
            "{}",
            0,
            "0 0 * * *");

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void GetId_ReturnsId()
    {
        var schedule = Create();
        var id = Guid.NewGuid();
        SetId(schedule, id);

        var result = schedule.GetId();

        result.Should().Be(id);
    }

    [Fact]
    public void GetKeySelector_ReturnsId()
    {
        var schedule = Create();
        var id = Guid.NewGuid();
        SetId(schedule, id);
        var selector = JobSchedule.GetKeySelector().Compile();

        var result = selector(schedule);

        result.Should().Be(id);
    }

    [Fact]
    public void GetEqualityExpression_SameId_ReturnsTrue()
    {
        var schedule = Create();
        var id = Guid.NewGuid();
        SetId(schedule, id);
        var predicate = JobSchedule.GetEqualityExpression(id).Compile();

        var result = predicate(schedule);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetEqualityExpression_DifferentId_ReturnsFalse()
    {
        var schedule = Create();
        SetId(schedule, Guid.NewGuid());
        var predicate = JobSchedule.GetEqualityExpression(Guid.NewGuid()).Compile();

        var result = predicate(schedule);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetDescription_BlankValue_BecomesNull(string? description)
    {
        var schedule = Create();

        schedule.SetDescription(description);

        schedule.Description.Should().BeNull();
    }

    [Fact]
    public void SetName_TrimsValue()
    {
        var schedule = Create();

        schedule.SetName("  New name  ");

        schedule.Name.Should().Be("New name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_BlankValue_Throws(string name)
    {
        var schedule = Create();

        var act = () => schedule.SetName(name);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetName_TooLong_Throws()
    {
        var schedule = Create();
        var name = new string('a', JobSchedule.NameMaxLength + 1);

        var act = () => schedule.SetName(name);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetDescription_TrimsValue()
    {
        var schedule = Create();

        schedule.SetDescription("  New description  ");

        schedule.Description.Should().Be("New description");
    }

    [Fact]
    public void SetDescription_TooLong_Throws()
    {
        var schedule = Create();
        var description = new string('a', JobSchedule.DescriptionMaxLength + 1);

        var act = () => schedule.SetDescription(description);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetCron_TrimsValue()
    {
        var schedule = Create();

        schedule.SetCron("  */5 * * * *  ");

        schedule.Cron.Should().Be("*/5 * * * *");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetCron_BlankValue_Throws(string cron)
    {
        var schedule = Create();

        var act = () => schedule.SetCron(cron);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetCron_NullValue_Throws()
    {
        var schedule = Create();

        var act = () => schedule.SetCron(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetInputState_TrimsValue()
    {
        var schedule = Create();

        schedule.SetInputState("  {\"page\":2}  ");

        schedule.InputState.Should().Be("{\"page\":2}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetInputState_BlankValue_Throws(string inputState)
    {
        var schedule = Create();

        var act = () => schedule.SetInputState(inputState);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetMaxAttempts_ValidValue_ChangesValue()
    {
        var schedule = Create();

        schedule.SetMaxAttempts(10);

        schedule.MaxAttempts.Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetMaxAttempts_LessOrEqualZero_Throws(int maxAttempts)
    {
        var schedule = Create();

        var act = () => schedule.SetMaxAttempts(maxAttempts);

        act.Should().Throw<InvalidInputException>();
    }

    [Fact]
    public void SetNextRunAt_ChangesValue()
    {
        var schedule = Create();
        var nextRunAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc);

        schedule.SetNextRunAt(nextRunAt);

        schedule.NextRunAt.Should().Be(nextRunAt);
    }

    [Fact]
    public void SetNextRunAt_NullValue_ClearsValue()
    {
        var schedule = Create();
        schedule.SetNextRunAt(DateTime.UtcNow);

        schedule.SetNextRunAt(null);

        schedule.NextRunAt.Should().BeNull();
    }

    [Fact]
    public void MarkQueued_ChangesLastQueuedAtAndNextRunAt()
    {
        var schedule = Create();
        var queuedAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc);
        var nextRunAt = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc);

        schedule.MarkQueued(queuedAt, nextRunAt);

        schedule.LastQueuedAt.Should().Be(queuedAt);
        schedule.NextRunAt.Should().Be(nextRunAt);
    }

    [Fact]
    public void MarkQueued_NullNextRunAt_ClearsNextRunAt()
    {
        var schedule = Create();
        var queuedAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc);

        schedule.MarkQueued(queuedAt, null);

        schedule.LastQueuedAt.Should().Be(queuedAt);
        schedule.NextRunAt.Should().BeNull();
    }

    [Fact]
    public void Enable_SetsEnabledTrue()
    {
        var schedule = Create();

        schedule.Enable();

        schedule.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Disable_SetsEnabledFalse()
    {
        var schedule = Create();
        schedule.Enable();

        schedule.Disable();

        schedule.Enabled.Should().BeFalse();
    }

    [Fact]
    public void AddScheduleRun_PersistedSchedule_AddsRun()
    {
        var schedule = Create();
        var scheduleId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var scheduledAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc);
        var queuedAt = new DateTime(2026, 7, 4, 10, 1, 0, DateTimeKind.Utc);
        SetId(schedule, scheduleId);

        schedule.AddScheduleRun(jobId, scheduledAt, queuedAt);

        schedule.Runs.Should().ContainSingle();
        var run = schedule.Runs.Single();
        run.JobScheduleId.Should().Be(scheduleId);
        run.JobId.Should().Be(jobId);
        run.ScheduledAt.Should().Be(scheduledAt);
        run.QueuedAt.Should().Be(queuedAt);
    }

    [Fact]
    public void AddScheduleRun_NewSchedule_Throws()
    {
        var schedule = Create();

        var act = () => schedule.AddScheduleRun(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddScheduleRun_EmptyJobId_Throws()
    {
        var schedule = Create();
        SetId(schedule, Guid.NewGuid());

        var act = () => schedule.AddScheduleRun(
            Guid.Empty,
            DateTime.UtcNow,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    private static JobSchedule Create()
    {
        return JobSchedule.Create(
            "Daily import",
            "Imports products",
            "import-products",
            "{}",
            3,
            "0 0 * * *");
    }

    private static void SetId(JobSchedule schedule, Guid id)
    {
        typeof(JobSchedule)
            .GetProperty(nameof(JobSchedule.Id))!
            .SetValue(schedule, id);
    }
}
