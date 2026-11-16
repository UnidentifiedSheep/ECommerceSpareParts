using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Modules.Jobs.Application.LRT;
using Microsoft.Extensions.DependencyInjection;
using NamedObject.Core.Interfaces;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Jobs.Lrt;

public class LrtJobAcquirerTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);

	[Fact]
	public async Task NoPendingJobs_ReturnsNull()
	{
		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().BeNull();
	}

	[Fact]
	public async Task OldestJobWins_WhenAllGroupsHaveCapacity()
	{
		var isolated = await AddJob(QuotaTestLrt.LrtName);
		await AddJob(JobScheduleTestLrt.LrtName);

		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().NotBeNull();
		acquired.Job.Id.Should().Be(isolated.Id);
		acquired.Lease.Dispose();
	}

	[Fact]
	public async Task FullGroup_IsSkipped_AndOtherGroupsStillRun()
	{
		await AddJob(QuotaTestLrt.LrtName);
		var other = await AddJob(JobScheduleTestLrt.LrtName);
		using var held = Quotas().GetBySystemName(QuotaTestLrt.QuotaName).UseQuota(Guid.NewGuid());

		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().NotBeNull();
		acquired.Job.Id.Should().Be(other.Id);
		acquired.Job.SystemName.Should().Be(JobScheduleTestLrt.LrtName);
		acquired.Lease.Dispose();
	}

	[Fact]
	public async Task FullGroup_WithNoOtherJobs_ReturnsNullAndLeavesJobPending()
	{
		var job = await AddJob(QuotaTestLrt.LrtName);
		using var held = Quotas().GetBySystemName(QuotaTestLrt.QuotaName).UseQuota(Guid.NewGuid());

		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().BeNull();
		Context.ChangeTracker.Clear();
		(await Context.Set<Job>().FindAsync([job.Id], CancellationToken))!.Status.Should().Be(JobStatus.Pending);
	}

	[Fact]
	public async Task Acquire_TakesASlotInTheJobsGroup_AndReleaseFreesIt()
	{
		await AddJob(QuotaTestLrt.LrtName);
		var manager = Quotas().GetBySystemName(QuotaTestLrt.QuotaName);

		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().NotBeNull();
		manager.AvailableQuota.Should().Be(0);
		acquired.Lease.Dispose();
		manager.AvailableQuota.Should().Be(1);
	}

	[Fact]
	public async Task JobOfUnregisteredLrt_IsNotAcquired()
	{
		await AddJob("unknown-lrt-name");

		var acquired = await Acquirer().TryAcquireAsync(Guid.NewGuid(), Lease, CancellationToken);

		acquired.Should().BeNull();
	}

	private LrtJobAcquirer Acquirer() => Scope.ServiceProvider.GetRequiredService<LrtJobAcquirer>();

	private INamedObjectRegistry<ILrtQuotaManager> Quotas()
		=> Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuotaManager>>();

	private async Task<Job> AddJob(string systemName)
	{
		var job = SingleRunJob.Create(systemName, "{}", 1);
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		return job;
	}
}
