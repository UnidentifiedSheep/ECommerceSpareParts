using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Application.Common.Models.Options;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NamedObject.Core.Interfaces;
using Tests.TestContainers.Combined;

namespace Tests.Jobs.Lrt;

public class LrtQuotaRegistrationTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void DefaultQuota_IsRegisteredAndUsesWorkerLimit()
	{
		var quotas = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuota>>();
		var limit = Scope.ServiceProvider.GetRequiredService<IOptions<LrtExecutorOptions>>().Value
			.MaxParallelPerWorker;

		var quota = quotas.GetBySystemName(DefaultLrtQuota.SystemName);

		quota.MaxParallelPerWorker.Should().Be(limit);
	}

	[Fact]
	public void EveryRegisteredLrt_ReferencesAnExistingQuota()
	{
		var quotas = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtQuota>>();
		var lrts = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>();

		var missing = lrts.All
			.OfType<IQuotaBoundLrt>()
			.Where(lrt => quotas.TryGetBySystemName(lrt.QuotaSystemName) is null)
			.Select(lrt => ((ILrtNamedObject)lrt).SystemName)
			.ToArray();

		missing.Should().BeEmpty();
	}

	[Fact]
	public void LrtBase_UsesDefaultQuotaUnlessOverridden()
	{
		var lrts = Scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>();

		lrts.All.OfType<IQuotaBoundLrt>().Should().NotBeEmpty();
		lrts.All.OfType<IQuotaBoundLrt>()
			.Should()
			.OnlyContain(lrt => lrt.QuotaSystemName == DefaultLrtQuota.SystemName);
	}
}
