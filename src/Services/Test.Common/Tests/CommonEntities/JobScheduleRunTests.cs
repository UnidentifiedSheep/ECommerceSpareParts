using Domain.CommonEntities;
using FluentAssertions;

namespace Tests.Tests.CommonEntities;

public class JobScheduleRunTests
{
    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var jobScheduleId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var scheduledAt = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc);
        var queuedAt = new DateTime(2026, 7, 4, 10, 1, 0, DateTimeKind.Utc);

        var run = JobScheduleRun.Create(
            jobScheduleId,
            jobId,
            scheduledAt,
            queuedAt);

        run.JobScheduleId.Should().Be(jobScheduleId);
        run.JobId.Should().Be(jobId);
        run.ScheduledAt.Should().Be(scheduledAt);
        run.QueuedAt.Should().Be(queuedAt);
    }

    [Fact]
    public void Create_EmptyJobScheduleId_Throws()
    {
        var act = () => JobScheduleRun.Create(
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyJobId_Throws()
    {
        var act = () => JobScheduleRun.Create(
            Guid.NewGuid(),
            Guid.Empty,
            DateTime.UtcNow,
            DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetJobScheduleId_ValidValue_ChangesValue()
    {
        var run = Create();
        var jobScheduleId = Guid.NewGuid();

        run.SetJobScheduleId(jobScheduleId);

        run.JobScheduleId.Should().Be(jobScheduleId);
    }

    [Fact]
    public void SetJobScheduleId_EmptyValue_Throws()
    {
        var run = Create();

        var act = () => run.SetJobScheduleId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetJobId_ValidValue_ChangesValue()
    {
        var run = Create();
        var jobId = Guid.NewGuid();

        run.SetJobId(jobId);

        run.JobId.Should().Be(jobId);
    }

    [Fact]
    public void SetJobId_EmptyValue_Throws()
    {
        var run = Create();

        var act = () => run.SetJobId(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetScheduledAt_ChangesValue()
    {
        var run = Create();
        var scheduledAt = new DateTime(2026, 7, 5, 10, 0, 0, DateTimeKind.Utc);

        run.SetScheduledAt(scheduledAt);

        run.ScheduledAt.Should().Be(scheduledAt);
    }

    [Fact]
    public void SetQueuedAt_ChangesValue()
    {
        var run = Create();
        var queuedAt = new DateTime(2026, 7, 5, 10, 1, 0, DateTimeKind.Utc);

        run.SetQueuedAt(queuedAt);

        run.QueuedAt.Should().Be(queuedAt);
    }

    [Fact]
    public void GetId_ReturnsId()
    {
        var run = Create();
        var id = Guid.NewGuid();
        SetId(run, id);

        var result = run.GetId();

        result.Should().Be(id);
    }

    [Fact]
    public void GetKeySelector_ReturnsId()
    {
        var run = Create();
        var id = Guid.NewGuid();
        SetId(run, id);
        var selector = JobScheduleRun.GetKeySelector().Compile();

        var result = selector(run);

        result.Should().Be(id);
    }

    [Fact]
    public void GetEqualityExpression_SameId_ReturnsTrue()
    {
        var run = Create();
        var id = Guid.NewGuid();
        SetId(run, id);
        var predicate = JobScheduleRun.GetEqualityExpression(id).Compile();

        var result = predicate(run);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetEqualityExpression_DifferentId_ReturnsFalse()
    {
        var run = Create();
        SetId(run, Guid.NewGuid());
        var predicate = JobScheduleRun.GetEqualityExpression(Guid.NewGuid()).Compile();

        var result = predicate(run);

        result.Should().BeFalse();
    }

    private static JobScheduleRun Create()
    {
        return JobScheduleRun.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 4, 10, 1, 0, DateTimeKind.Utc));
    }

    private static void SetId(JobScheduleRun run, Guid id)
    {
        typeof(JobScheduleRun)
            .GetProperty(nameof(JobScheduleRun.Id))!
            .SetValue(run, id);
    }
}
