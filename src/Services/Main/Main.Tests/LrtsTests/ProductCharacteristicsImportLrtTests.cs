using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Domain.CommonEntities.Job;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductCharacteristicsImport;
using Main.Application.Lrts.ProductSynchronization;
using Main.Enums;
using Microsoft.Extensions.DependencyInjection;
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
		job.Steps[0].State.Should().Be(input);
		JsonSerializer.Deserialize<ProductSynchronizationInputState>(job.Steps[1].State)!.Parts
			.Should().Be(ProductEventParts.Characteristics);
		var dependency = job.Dependencies.Should().ContainSingle().Which;
		dependency.StepId.Should().Be(job.Steps[1].Id);
		dependency.DependsOnStepId.Should().Be(job.Steps[0].Id);
	}
}
