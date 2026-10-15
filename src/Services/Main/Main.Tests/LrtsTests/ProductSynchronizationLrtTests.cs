using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.LRT;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.Base.Synchronization;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities.Product;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductSynchronizationLrtTests : IntegrationTest
{
	public ProductSynchronizationLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task EmptyInput_PublishesAllSectionsAndPersistsTheCursor()
	{
		var broker = new MessageBrokerStub();

		var job = await ExecuteSynchronization("{}", broker);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		broker.PublishedMessages.Should().HaveCount(4);
		broker.PublishedMessagesOfType<ProductsUpdatedEvent>().Should().ContainSingle().Which.Products
			.Should().HaveCount(TestContext.Products.Count);
		broker.PublishedMessagesOfType<ProductCharacteristicsUpdatedEvent>().Should().ContainSingle();
		broker.PublishedMessagesOfType<ProductDimensionsUpdatedEvent>().Should().ContainSingle();
		broker.PublishedMessagesOfType<ProductWeightUpdatedEvent>().Should().ContainSingle();
		var (input, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		input.Parts.Should().Be(ProductEventParts.All);
		state!.LastProcessedId.Should().Be(TestContext.Products.Max(product => product.Id));
		state.ProcessedCount.Should().Be(TestContext.Products.Count);
	}

	[Fact]
	public async Task ResumedSynchronization_PreservesSelectedPartsAndPublishesEmptyCharacteristics()
	{
		var products = TestContext.Products.OrderBy(product => product.Id).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.AddRange(
			ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Red"),
			ProductCharacteristic.Create(products[1].Id, characteristic.Id, "Blue"));
		await Context.SaveChangesAsync(CancellationToken);
		var broker = new MessageBrokerStub();
		var input = JsonSerializer.Serialize(new ProductSynchronizationInputState { Parts = ProductEventParts.Characteristics });

		var job = await ExecuteSynchronization(input, broker,
			new SynchronizationState<int> { LastProcessedId = products[0].Id, ProcessedCount = 1 });

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var notification = broker.PublishedMessages.Should().ContainSingle().Which.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		notification.Products.Select(product => product.ProductId).Should().Equal(products.Skip(1).Select(product => product.Id));
		notification.Products.Single(product => product.ProductId == products[1].Id).Characteristics
			.Should().ContainSingle().Which.Value.Should().Be("Blue");
		notification.Products.Single(product => product.ProductId == products[2].Id).Characteristics.Should().BeEmpty();
		var (savedInput, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		savedInput.Parts.Should().Be(ProductEventParts.Characteristics);
		state!.LastProcessedId.Should().Be(products[^1].Id);
		state.ProcessedCount.Should().Be(products.Length);
	}

	private async Task<Job> ExecuteSynchronization(
		string input,
		MessageBrokerStub broker,
		SynchronizationState<int>? checkpoint = null)
	{
		var lrt = ActivatorUtilities.CreateInstance<ProductSynchronizationLrt>(Scope.ServiceProvider, broker);
		var validatedInput = lrt.ValidateState(input);
		var jobState = checkpoint is null
			? validatedInput
			: LrtStateSerializer.Serialize(JsonSerializer.Deserialize<ProductSynchronizationInputState>(validatedInput)!, checkpoint);
		var job = SingleRunJob.Create(lrt.SystemName, jobState, 3);
		var leaseHolderId = Guid.NewGuid();
		job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await lrt.ExecuteAsync(job.Id, leaseHolderId, CancellationToken);

		await Context.Entry(job).ReloadAsync(CancellationToken);
		return job;
	}
}
