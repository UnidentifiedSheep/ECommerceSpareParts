using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.LRT;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductCharacteristicsImport;
using Main.Application.Lrts.ProductSynchronization;
using Main.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Main.Persistence.Context;
using NamedObject.Core.Interfaces;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public sealed class ProductCharacteristicsImportLrtTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
	[Fact]
	public void CreateJob_ImportsBeforeSynchronizingOnlyCharacteristics()
	{
		var input = JsonSerializer.Serialize(new ProductCharacteristicsImportInputState { FileName = "characteristics.csv" });
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();

		var job = dispatcher.Create(ProductCharacteristicsImportLrt.Name, input, 3)
			.Should().BeOfType<MultiStepJob>().Subject;

		job.Steps.Select(step => step.SystemName).Should()
			.Equal(ProductCharacteristicsImportLrtStep.Name, ProductSynchronizationLrt.Name);
		job.Steps[0].InputInitialized.Should().BeFalse();
		job.Steps[0].StepKey.Should().Be(ProductCharacteristicsImportLrtStep.Name);
		job.Steps[1].InputInitialized.Should().BeFalse();
		var dependency = job.Dependencies.Should().ContainSingle().Which;
		dependency.StepId.Should().Be(job.Steps[1].Id);
		dependency.DependsOnStepId.Should().Be(job.Steps[0].Id);
	}

	[Fact]
	public async Task Execute_DeferredSynchronizationInputContainsOnlyCharacteristics()
	{
		const string fileName = "empty-characteristics.csv";
		Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>()
			.SetFile("uploads", fileName, "ProductId,Characteristic,Value\n");
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();
		var parent = (MultiStepJob)dispatcher.Create(ProductCharacteristicsImportLrt.Name,
			JsonSerializer.Serialize(new ProductCharacteristicsImportInputState { FileName = fileName }), 3);
		await Context.AddAsync(parent, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await ExecuteJobAsync(parent.Id);
		var import = parent.Steps[0];
		var synchronization = parent.Steps[1];
		await ExecuteJobAsync(import.Id);
		var imported = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == import.Id, CancellationToken);
		imported.Status.Should().Be(JobStatus.Succeeded, imported.ErrorMessage);
		LrtStateSerializer.DeserializeInput<ProductCharacteristicsImportInputState>(imported.State)
			.FileName.Should().Be(fileName);

		await ExecuteJobAsync(parent.Id);
		await ExecuteJobAsync(synchronization.Id);
		var synchronized = await Context.Jobs.AsNoTracking().SingleAsync(x => x.Id == synchronization.Id, CancellationToken);
		synchronized.Status.Should().Be(JobStatus.Succeeded, synchronized.ErrorMessage);
		synchronized.InputInitialized.Should().BeTrue();
		LrtStateSerializer.DeserializeInput<ProductSynchronizationInputState>(synchronized.State)
			.Parts.Should().Be(ProductEventParts.Characteristics);
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
