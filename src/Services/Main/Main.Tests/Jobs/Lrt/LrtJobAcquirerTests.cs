using Application.Common.Interfaces.Lrt;
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
	public async Task NoPendingJobs_ReturnsEmpty()
	{
		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().BeEmpty();
	}

	[Fact]
	public async Task EveryQuotaWithPendingJobs_IsServedInOneCall()
	{
		var isolated = await AddJob(QuotaTestLrt.LrtName);
		await AddJob(QuotaTestLrt.LrtName);
		var other = await AddJob(JobScheduleTestLrt.LrtName);

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Select(x => x.Job.Id).Should().BeEquivalentTo([isolated.Id, other.Id]);
		DisposeAll(acquired);
	}

	[Fact]
	public async Task TakesUpToAvailableQuota_OldestFirst()
	{
		var manager = Quotas().GetBySystemName(LrtQuotaRegistrations.Default);
		var jobs = new List<Job>();
		for (var i = 0; i < manager.MaxQuota + 2; i++)
			jobs.Add(await AddJob(JobScheduleTestLrt.LrtName));

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Select(x => x.Job.Id).Should().Equal(jobs.Take(manager.MaxQuota).Select(x => x.Id));
		manager.AvailableQuota.Should().Be(0);
		DisposeAll(acquired);
		manager.AvailableQuota.Should().Be(manager.MaxQuota);
	}

	[Fact]
	public async Task UnusedReservedQuota_IsReleased_WhenFewerJobsThanSlots()
	{
		var manager = Quotas().GetBySystemName(LrtQuotaRegistrations.Default);
		await AddJob(JobScheduleTestLrt.LrtName);

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().ContainSingle();
		manager.AvailableQuota.Should().Be(manager.MaxQuota - 1);
		DisposeAll(acquired);
		manager.AvailableQuota.Should().Be(manager.MaxQuota);
	}

	[Fact]
	public async Task QuotaTakenOutsideBeforeAcquire_TakesOnlyRemainingSlots()
	{
		var manager = Quotas().GetBySystemName(LrtQuotaRegistrations.Default);
		var jobs = await AddJobs(JobScheduleTestLrt.LrtName, manager.MaxQuota + 1);
		var external = manager.UseQuota(Guid.NewGuid());
		manager.AvailableQuota.Should().Be(manager.MaxQuota - 1);

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Select(x => x.Job.Id).Should().Equal(jobs.Take(manager.MaxQuota - 1).Select(x => x.Id));
		manager.AvailableQuota.Should().Be(0);
		await AssertPendingAsync(jobs.Skip(manager.MaxQuota - 1));

		DisposeAll(acquired);
		external.Dispose();
		manager.AvailableQuota.Should().Be(manager.MaxQuota);
	}

	[Fact]
	public async Task QuotaTakenOutsideDuringAcquire_TakesOnlyRemainingSlots()
	{
		var manager = Quotas().GetBySystemName(LrtQuotaRegistrations.Default);
		var jobs = await AddJobs(JobScheduleTestLrt.LrtName, manager.MaxQuota + 1);
		var leaseService = GrabbingLeaseService(manager, false);

		var acquired = await Acquirer(leaseService).AcquireAsync(Lease, CancellationToken);

		leaseService.Attempted.Should().BeTrue();
		leaseService.Grabbed.Should().NotBeNull();
		acquired.Select(x => x.Job.Id).Should().Equal(jobs.Take(manager.MaxQuota - 1).Select(x => x.Id));
		manager.AvailableQuota.Should().Be(0);
		await AssertPendingAsync(jobs.Skip(manager.MaxQuota - 1));

		DisposeAll(acquired);
		leaseService.Grabbed!.Dispose();
		manager.AvailableQuota.Should().Be(manager.MaxQuota);
	}

	[Fact]
	public async Task QuotaRequestedOutsideWhileJobsAreAcquired_IsDenied_AndAllReservedSlotsAreUsed()
	{
		var manager = Quotas().GetBySystemName(LrtQuotaRegistrations.Default);
		var jobs = await AddJobs(JobScheduleTestLrt.LrtName, manager.MaxQuota + 1);
		var leaseService = GrabbingLeaseService(manager, true);

		var acquired = await Acquirer(leaseService).AcquireAsync(Lease, CancellationToken);

		leaseService.Attempted.Should().BeTrue();
		leaseService.Grabbed.Should().BeNull();
		acquired.Select(x => x.Job.Id).Should().Equal(jobs.Take(manager.MaxQuota).Select(x => x.Id));
		manager.AvailableQuota.Should().Be(0);
		await AssertPendingAsync(jobs.Skip(manager.MaxQuota));

		DisposeAll(acquired);
		manager.AvailableQuota.Should().Be(manager.MaxQuota);
	}

	[Fact]
	public async Task EachAcquiredJob_GetsItsOwnLeaseHolder()
	{
		await AddJob(JobScheduleTestLrt.LrtName);
		await AddJob(JobScheduleTestLrt.LrtName);

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().HaveCount(2);
		acquired.Should().AllSatisfy(x => x.Lease.HolderId.Should().Be(x.Job.LeaseHolderId!.Value));
		acquired.Select(x => x.Lease.HolderId).Should().OnlyHaveUniqueItems();
		DisposeAll(acquired);
	}

	[Fact]
	public async Task FullGroup_IsSkipped_AndOtherGroupsStillRun()
	{
		await AddJob(QuotaTestLrt.LrtName);
		var other = await AddJob(JobScheduleTestLrt.LrtName);
		using var held = Quotas().GetBySystemName(QuotaTestLrt.QuotaName).UseQuota(Guid.NewGuid());

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().ContainSingle().Which.Job.Id.Should().Be(other.Id);
		DisposeAll(acquired);
	}

	[Fact]
	public async Task FullGroup_WithNoOtherJobs_ReturnsEmptyAndLeavesJobPending()
	{
		var job = await AddJob(QuotaTestLrt.LrtName);
		using var held = Quotas().GetBySystemName(QuotaTestLrt.QuotaName).UseQuota(Guid.NewGuid());

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().BeEmpty();
		Context.ChangeTracker.Clear();
		(await Context.Set<Job>().FindAsync([job.Id], CancellationToken))!.Status
			.Should()
			.Be(JobStatus.Pending);
	}

	[Fact]
	public async Task JobOfUnregisteredLrt_IsNotAcquired()
	{
		await AddJob("unknown-lrt-name");

		var acquired = await Acquirer().AcquireAsync(Lease, CancellationToken);

		acquired.Should().BeEmpty();
	}

	private static void DisposeAll(IEnumerable<AcquiredJob> acquired)
	{
		foreach (var item in acquired)
			item.Lease.Dispose();
	}

	private ILrtJobAcquirer Acquirer() => Scope.ServiceProvider.GetRequiredService<ILrtJobAcquirer>();

	private ILrtJobAcquirer Acquirer(IJobLeaseService leaseService)
		=> new LrtJobAcquirer(Scope.ServiceProvider.GetRequiredService<ILrtQuotaCatalog>(), leaseService);

	private QuotaGrabbingLeaseService GrabbingLeaseService(
		ILrtQuotaManager manager,
		bool grabDuringJobAcquisition)
		=> new(
			Scope.ServiceProvider.GetRequiredService<IJobLeaseService>(),
			manager,
			grabDuringJobAcquisition);

	private INamedObjectRegistry<ILrtQuotaManager> Quotas()
		=> Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuotaManager>>();

	private async Task<Job> AddJob(string systemName)
	{
		var job = SingleRunJob.Create(
			systemName,
			"{}",
			1);
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		return job;
	}
	private async Task<List<Job>> AddJobs(string systemName, int count)
	{
		var jobs = new List<Job>();
		for (var i = 0; i < count; i++)
			jobs.Add(await AddJob(systemName));

		return jobs;
	}

	private async Task AssertPendingAsync(IEnumerable<Job> jobs)
	{
		Context.ChangeTracker.Clear();
		foreach (var job in jobs)
			(await Context.Set<Job>().FindAsync([job.Id], CancellationToken))!.Status
				.Should()
				.Be(JobStatus.Pending);
	}

	private sealed class QuotaGrabbingLeaseService(
		IJobLeaseService inner,
		ILrtQuotaManager manager,
		bool grabDuringJobAcquisition) : IJobLeaseService
	{
		public bool Attempted { get; private set; }

		public ILrtQuotaLease? Grabbed { get; private set; }

		public async Task<IReadOnlyList<string>> GetAcquirableSystemNamesAsync(
			IReadOnlyCollection<string> systemNames,
			CancellationToken ct)
		{
			var result = await inner.GetAcquirableSystemNamesAsync(systemNames, ct);
			if (!grabDuringJobAcquisition)
				Grab();

			return result;
		}

		public Task<IReadOnlyList<Job>> AcquireJobsAsync(
			IReadOnlyCollection<string> systemNames,
			IReadOnlyList<Guid> holderIds,
			TimeSpan leaseDuration,
			CancellationToken ct)
		{
			if (grabDuringJobAcquisition)
				Grab();

			return inner.AcquireJobsAsync(
				systemNames,
				holderIds,
				leaseDuration,
				ct);
		}

		public Task<Job?> TryAcquireJobAsync(Guid holderId, TimeSpan leaseDuration, CancellationToken ct)
			=> inner.TryAcquireJobAsync(holderId, leaseDuration, ct);

		public Task<List<Job>> FailExpiredJobsWithoutAttempts(int maxBatchSize, CancellationToken ct)
			=> inner.FailExpiredJobsWithoutAttempts(maxBatchSize, ct);

		public Task<List<Job>> CancelExpiredJobs(int maxBatchSize, CancellationToken ct)
			=> inner.CancelExpiredJobs(maxBatchSize, ct);

		private void Grab()
		{
			if (Attempted)
				return;

			Attempted = true;
			if (manager.TryUseQuota(Guid.NewGuid(), out var lease))
				Grabbed = lease;
		}
	}
}
