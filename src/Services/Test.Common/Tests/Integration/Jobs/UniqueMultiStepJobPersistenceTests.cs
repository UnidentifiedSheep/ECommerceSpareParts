using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities.Job;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Integration;
using Tests.TestContainers.Combined;

namespace Tests.Tests.Integration.Jobs;

public sealed class UniqueMultiStepJobPersistenceTests(
    CombinedContainerFixture fixture)
    : CommonLayerIntegrationTest(fixture)
{
    [Fact]
    public async Task TryAddAsync_MultiStepJob_PersistsWholeGraph()
    {
        var job = CreateUniqueWorkflow("workflow:42");

        var addedCount = await AddJobsAsync(job);

        addedCount.Should().Be(1);

        Context.ChangeTracker.Clear();
        var persisted = await Context.Jobs
            .AsNoTracking()
            .OfType<MultiStepJob>()
            .Include(x => x.Steps)
            .Include(x => x.Dependencies)
            .SingleAsync(x => x.Id == job.Id);

        persisted.NaturalKey.Should().Be("workflow:42");
        persisted.Steps.Should().HaveCount(2)
            .And.OnlyContain(x => x.NaturalKey == null);
        persisted.Dependencies.Should().ContainSingle();
    }

    [Fact]
    public async Task TryAddAsync_DuplicateMultiStepJob_SkipsSecondGraph()
    {
        var first = CreateUniqueWorkflow("workflow:42");
        var duplicate = CreateUniqueWorkflow("workflow:42");

        var firstAddedCount = await AddJobsAsync(first);
        Context.ChangeTracker.Clear();
        var duplicateAddedCount = await AddJobsAsync(duplicate);

        firstAddedCount.Should().Be(1);
        duplicateAddedCount.Should().Be(0);

        Context.ChangeTracker.Clear();
        var roots = await Context.Jobs
            .AsNoTracking()
            .Where(x => x.MultiStepJobId == null)
            .Where(x => x.SystemName == "workflow")
            .Where(x => x.NaturalKey == "workflow:42")
            .ToListAsync();

        roots.Should().ContainSingle()
            .Which.Id.Should().Be(first.Id);
        (await Context.Jobs.AnyAsync(x => x.Id == duplicate.Id))
            .Should().BeFalse();
    }

    [Fact]
    public async Task TryAddAsync_ConcurrentDuplicates_AddsOnlyOneGraph()
    {
        using var firstScope = Sp.CreateScope();
        using var secondScope = Sp.CreateScope();

        var results = await Task.WhenAll(
            AddJobsAsync(
                firstScope.ServiceProvider,
                [
                    CreateUniqueWorkflow("workflow:42"),
                    CreateUniqueWorkflow("workflow:43")
                ]),
            AddJobsAsync(
                secondScope.ServiceProvider,
                [
                    CreateUniqueWorkflow("workflow:43"),
                    CreateUniqueWorkflow("workflow:42")
                ]));

        results.Should().BeEquivalentTo([0, 2]);

        Context.ChangeTracker.Clear();
        var rootCount = await Context.Jobs
            .AsNoTracking()
            .CountAsync(x =>
                x.MultiStepJobId == null &&
                x.SystemName == "workflow" &&
                (x.NaturalKey == "workflow:42" ||
                 x.NaturalKey == "workflow:43"));
        var stepCount = await Context.Jobs
            .AsNoTracking()
            .CountAsync(x => x.MultiStepJobId != null);

        rootCount.Should().Be(2);
        stepCount.Should().Be(4);
    }

    [Fact]
    public async Task TryAddAsync_MixedBatch_PersistsSingleRunAndMultiStepJobs()
    {
        var singleRunJob = SingleRunJob.CreateUnique(
            "single:42",
            "single-run",
            "{}");
        var multiStepJob = CreateUniqueWorkflow("workflow:42");

        var addedCount = await AddJobsAsync([singleRunJob, multiStepJob]);

        addedCount.Should().Be(2);

        Context.ChangeTracker.Clear();
        (await Context.Jobs.AnyAsync(x => x.Id == singleRunJob.Id))
            .Should().BeTrue();
        var persistedWorkflow = await Context.Jobs
            .AsNoTracking()
            .OfType<MultiStepJob>()
            .Include(x => x.Steps)
            .SingleAsync(x => x.Id == multiStepJob.Id);
        persistedWorkflow.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task TryAddAsync_PartialSingleRunConflict_ReturnsOnlyInsertedId()
    {
        var existing = SingleRunJob.CreateUnique(
            "single:42",
            "single-run",
            "{}");
        await AddJobsAsync(existing);
        Context.ChangeTracker.Clear();

        var duplicate = SingleRunJob.CreateUnique(
            "single:42",
            "single-run",
            "{}");
        var newJob = SingleRunJob.CreateUnique(
            "single:43",
            "single-run",
            "{}");

        var addedIds = await AddJobIdsAsync([duplicate, newJob]);

        addedIds.Should().ContainSingle()
            .Which.Should().Be(newJob.Id);
    }

    [Fact]
    public async Task InsertJobsAsync_NonUniqueBatch_PersistsEveryJob()
    {
        var first = SingleRunJob.Create("single-run", "{}");
        var second = SingleRunJob.Create("single-run", "{}");
        var workflow = MultiStepJob.Create("workflow", "{}");
        var step = SingleRunJob.Create("workflow-step", "{}");
        workflow.AddStep(step);

        var addedIds = await AddJobIdsAsync([first, second, workflow]);

        addedIds.Should().BeEquivalentTo([
            first.Id,
            second.Id,
            workflow.Id
        ]);

        Context.ChangeTracker.Clear();
        var persistedIds = await Context.Jobs
            .AsNoTracking()
            .Where(x =>
                x.Id == first.Id ||
                x.Id == second.Id ||
                x.Id == workflow.Id ||
                x.Id == step.Id)
            .Select(x => x.Id)
            .ToListAsync();

        persistedIds.Should().BeEquivalentTo([
            first.Id,
            second.Id,
            workflow.Id,
            step.Id
        ]);
    }

    private Task<int> AddJobsAsync(Job job)
    {
        return AddJobsAsync([job]);
    }

    private async Task<int> AddJobsAsync(IEnumerable<Job> jobs)
    {
        var addedIds = await AddJobIdsAsync(jobs);
        return addedIds.Count;
    }

    private Task<IReadOnlyList<Guid>> AddJobIdsAsync(
        IEnumerable<Job> jobs)
    {
        return AddJobIdsAsync(Scope.ServiceProvider, jobs);
    }

    private static async Task<int> AddJobsAsync(
        IServiceProvider serviceProvider,
        Job job)
    {
        return await AddJobsAsync(serviceProvider, [job]);
    }

    private static async Task<int> AddJobsAsync(
        IServiceProvider serviceProvider,
        IEnumerable<Job> jobs)
    {
        var addedIds = await AddJobIdsAsync(serviceProvider, jobs);
        return addedIds.Count;
    }

    private static async Task<IReadOnlyList<Guid>> AddJobIdsAsync(
        IServiceProvider serviceProvider,
        IEnumerable<Job> jobs)
    {
        var transactionService = serviceProvider
            .GetRequiredService<IApplicationTransactionService>();

        return await transactionService.ExecuteAsync(
            new TransactionalAttribute(),
            async (context, cancellationToken) =>
            {
                var repository = context.Repositories.Get<IJobRepository>();
                var addedIds = await repository.InsertJobsAsync(
                    jobs,
                    cancellationToken);

                if (addedIds.Count != 0)
                    await context.UnitOfWork.SaveChangesAsync(cancellationToken);

                return addedIds;
            });
    }

    private static MultiStepJob CreateUniqueWorkflow(string naturalKey)
    {
        var job = MultiStepJob.CreateUnique(
            naturalKey,
            "workflow",
            "{}");
        var firstStep = SingleRunJob.Create("first-step", "{}");
        var secondStep = SingleRunJob.Create("second-step", "{}");

        job.AddStep(firstStep);
        job.AddStep(secondStep);
        job.AddDependency(secondStep, firstStep);

        return job;
    }
}
