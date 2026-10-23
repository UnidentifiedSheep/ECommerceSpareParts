using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductImport;
using Main.Application.Lrts.ProductSynchronization;
using Main.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Main.Persistence.Context;
using NamedObject.Core.Interfaces;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class ProductImportLrtTests(CombinedContainerFixture fixture)
	: IntegrationTest(fixture)
{
	[Fact]
	public void CreateJob_ImportsBeforeSynchronizing()
	{
		const string fileName = "products.csv";
		var input = JsonSerializer.Serialize(new ProductImportInputState { FileName = fileName });
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();

		var job = dispatcher.Create(nameof(ProductImportLrt), input, 3)
			.Should().BeOfType<MultiStepJob>().Subject;

		job.Steps.Select(x => x.SystemName).Should()
			.Equal(nameof(ProductImportLrtStep), nameof(ProductSynchronizationLrt));
		job.Steps[0].InputInitialized.Should().BeFalse();
		job.Steps[0].StepKey.Should().Be(nameof(ProductImportLrtStep));
		job.Steps[1].InputInitialized.Should().BeFalse();
		job.Dependencies.Should().ContainSingle()
			.Which.StepId.Should().Be(job.Steps[1].Id);
		job.Dependencies[0].DependsOnStepId.Should().Be(job.Steps[0].Id);
	}

	[Fact]
	public async Task Execute_DeferredImportInputIsPreparedFromParentBeforeReadingCsv()
	{
		const string fileName = "empty-products.csv";
		Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>()
			.SetFile("uploads", fileName, "Sku,Name,Producer\n");
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();
		var parent = (MultiStepJob)dispatcher.Create(nameof(ProductImportLrt),
			JsonSerializer.Serialize(new ProductImportInputState { FileName = fileName }), 3);
		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await ExecuteJobAsync(parent.Id);
		var steps = await Context.Jobs.AsNoTracking().Where(x => x.MultiStepJobId == parent.Id).ToListAsync(CancellationToken);
		var import = steps.Single(x => x.SystemName == nameof(ProductImportLrtStep));
		var synchronization = steps.Single(x => x.SystemName == nameof(ProductSynchronizationLrt));
		import.InputInitialized.Should().BeFalse();
		synchronization.Status.Should().Be(JobStatus.Blocked);

		await ExecuteJobAsync(import.Id);
		var imported = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == import.Id, CancellationToken);
		imported.Status.Should().Be(JobStatus.Succeeded, imported.ErrorMessage);
		imported.InputInitialized.Should().BeTrue();
		LrtStateSerializer.DeserializeInput<ProductImportInputState>(imported.State)
			.FileName.Should().Be(fileName);

		await ExecuteJobAsync(parent.Id);
		await ExecuteJobAsync(synchronization.Id);
		var synchronized = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == synchronization.Id, CancellationToken);
		synchronized.Status.Should().Be(JobStatus.Succeeded, synchronized.ErrorMessage);
		synchronized.InputInitialized.Should().BeTrue();
		LrtStateSerializer.DeserializeInput<ProductSynchronizationInputState>(synchronized.State)
			.Parts.Should().Be(ProductEventParts.All);
		await ExecuteJobAsync(parent.Id);

		var completed = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == parent.Id, CancellationToken);
		completed.Status.Should().Be(JobStatus.Succeeded, completed.ErrorMessage);
	}

	private async Task ExecuteJobAsync(Guid jobId)
	{
		using var scope = Sp.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<DContext>();
		var job = await context.Jobs.SingleAsync(x => x.Id == jobId, CancellationToken);
		var lease = Guid.NewGuid();
		job.AcquireLease(lease, TimeSpan.FromMinutes(5));
		await context.SaveChangesAsync(CancellationToken);
		var lrt = scope.ServiceProvider.GetRequiredService<INamedObjectRegistry<ILrtNamedObject>>()
			.GetBySystemName(job.SystemName);
		await lrt.ExecuteAsync(job.Id, lease, CancellationToken);
	}
}
