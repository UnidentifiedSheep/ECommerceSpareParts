using Api.Common.HostedServices;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Application.Common.Models.Lrt;
using FluentAssertions;
using Main.Modules.Jobs.Application.HostedServices;
using Main.Modules.Jobs.Application.LRT;
using Main.Modules.Jobs.Application.Models.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NamedObject.Core.Interfaces;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.Jobs.Lrt;

public class LrtQuotaRegistrationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void DefaultQuota_IsRegisteredAndUsesWorkerLimit()
	{
		var quotas = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuotaManager>>();
		var limit = Scope.ServiceProvider.GetRequiredService<IOptions<LrtExecutorOptions>>().Value
			.MaxParallelPerWorker;

		var manager = quotas.GetBySystemName(LrtQuotaRegistrations.Default);

		manager.SystemName.Should().Be(LrtQuotaRegistrations.Default);
		manager.MaxQuota.Should().Be(limit);
	}

	[Fact]
	public void RegisteredGroup_IsAvailableByName()
	{
		var quotas = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuotaManager>>();

		var manager = quotas.GetBySystemName(QuotaTestLrt.QuotaName);

		manager.MaxQuota.Should().Be(1);
	}

	[Fact]
	public void EveryRegisteredLrt_ReferencesAnExistingQuota()
	{
		var catalog = Scope.ServiceProvider.GetRequiredService<LrtQuotaCatalog>();

		var act = catalog.Validate;

		act.Should().NotThrow();
		catalog.ManagersByLrt[QuotaTestLrt.LrtName].SystemName.Should().Be(QuotaTestLrt.QuotaName);
		catalog.ManagersByLrt[JobScheduleTestLrt.LrtName].SystemName.Should().Be(LrtQuotaRegistrations.Default);
	}

	[Fact]
	public void Registry_DuplicateGroup_Throws()
	{
		var act = () => new LrtQuotaRegistry(
			[new LrtQuotaRegistration("pricing", _ => 10), new LrtQuotaRegistration("pricing", _ => 20)],
			new ServiceCollection().BuildServiceProvider());

		act.Should().Throw<InvalidOperationException>().WithMessage("*pricing*");
	}

	[Fact]
	public void Registry_RegisteringDefaultTwice_Throws()
	{
		var services = new ServiceCollection()
			.AddOptions()
			.AddDefaultLrtQuotas()
			.AddLrtQuota(LrtQuotaRegistrations.Default, 10);
		var provider = services.BuildServiceProvider();

		var act = () => new LrtQuotaRegistry(
			provider.GetServices<LrtQuotaRegistration>(),
			provider);

		act.Should().Throw<InvalidOperationException>().WithMessage("*default*");
	}

	[Fact]
	public void Registry_ResolvesLimitFromServicesAtCreation()
	{
		var services = new ServiceCollection()
			.AddOptions()
			.Configure<LrtExecutorOptions>(options => options.MaxParallelPerWorker = 7)
			.AddDefaultLrtQuotas()
			.AddLrtQuota("pricing", 10000);
		var provider = services.BuildServiceProvider();

		var registry = new LrtQuotaRegistry(provider.GetServices<LrtQuotaRegistration>(), provider);

		registry.GetBySystemName(LrtQuotaRegistrations.Default).MaxQuota.Should().Be(7);
		registry.GetBySystemName("pricing").MaxQuota.Should().Be(10000);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void AddLrtQuota_EmptyName_Throws(string name)
	{
		var act = () => new ServiceCollection().AddLrtQuota(name, 5);

		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-3)]
	public void AddLrtQuota_NonPositiveLimit_Throws(int limit)
	{
		var act = () => new ServiceCollection().AddLrtQuota("pricing", limit);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async Task ReleasingLease_WakesTheExecutorSignal()
	{
		var signal = new JobSignal();
		var services = new ServiceCollection().AddSingleton<IJobSignal>(signal).BuildServiceProvider();
		var registry = new LrtQuotaRegistry([new LrtQuotaRegistration("signal-group", _ => 1)], services);
		var lease = registry.GetBySystemName("signal-group").UseQuota(Guid.NewGuid());
		var started = DateTime.UtcNow;

		lease.Dispose();
		await signal.WaitAsync(TimeSpan.FromSeconds(30), CancellationToken);

		(DateTime.UtcNow - started).Should().BeLessThan(TimeSpan.FromSeconds(5));
	}
}
