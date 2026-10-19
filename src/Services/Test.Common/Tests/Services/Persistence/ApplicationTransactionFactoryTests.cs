using Application.Common.Extensions;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Services.Events;
using Application.Common.Services.Persistence;
using Domain.Interfaces.Events;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.Stubs;

namespace Tests.Tests.Services.Persistence;

public sealed class ApplicationTransactionFactoryTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Factory_IsolatesBatchesAndDisposesTheirDependencies(bool fail)
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		services.AddScoped(_ => Mock.Of<IUnitOfWork>());
		services.AddScoped(_ => Mock.Of<IRepositoryProvider>());
		services.AddScoped<IIntegrationEventScope, IntegrationEventScope>();
		services.AddScoped<IDomainEventExecutor, DomainEventExecutorStub>();
		services.AddScoped<IPublishEndpoint, MessageBrokerStub>();
		services.AddSingleton(Mock.Of<ITransactionExceptionClassifier>());
		services.AddScoped<IDomainEventScope>(_ => Mock.Of<IDomainEventScope>());
		services.AddScoped<BatchResource>();
		await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
		using var request = provider.CreateScope();
		var requestTransaction = request.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		request.ServiceProvider.GetRequiredService<IApplicationTransaction>().Should().BeSameAs(requestTransaction);
		var factory = provider.GetRequiredService<IApplicationTransactionFactory>();
		var batches = new List<BatchResource>();
		var contexts = new List<IApplicationTransactionContext>();
		var units = new List<IUnitOfWork>();

		for (var batch = 0; batch < 20; batch++)
		{
			await using var transaction = factory.Create();
			var operation = () => transaction.ExecuteAsync(async (context, ct) =>
			{
				contexts.Add(context);
				units.Add(context.UnitOfWork);
				var resource = context.GetRequiredService<BatchResource>();
				batches.Add(resource);
				context.GetRequiredService<BatchResource>().Should().BeSameAs(resource);
				context.GetRequiredService<IApplicationTransaction>().IsExecuting.Should().BeTrue();
				context.GetRequiredService<IApplicationTransaction>().Should().BeSameAs(transaction);
				context.GetRequiredService<IApplicationTransaction>().Should().NotBeSameAs(requestTransaction);
				context.Publisher.Should().BeSameAs(context.GetRequiredService<IPublishEndpoint>());
				resource.Disposed.Should().BeFalse();
				if (fail) throw new InvalidOperationException("batch failed");
				await Task.CompletedTask;
			}, TestContext.Current.CancellationToken);
			if (fail) await operation.Should().ThrowAsync<InvalidOperationException>();
			else await operation();
		}

		batches.Should().OnlyContain(resource => resource.Disposed && resource.DisposeCalls == 1);
		contexts.Distinct().Should().HaveCount(20);
		units.Distinct().Should().HaveCount(20);
	}

	[Fact]
	public async Task Context_CanResolveScopedServicesBeforeStartingTheTransaction()
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		services.AddScoped(_ => Mock.Of<IUnitOfWork>());
		services.AddScoped(_ => Mock.Of<IRepositoryProvider>());
		services.AddScoped<IIntegrationEventScope, IntegrationEventScope>();
		services.AddScoped<IDomainEventExecutor, DomainEventExecutorStub>();
		services.AddScoped<IPublishEndpoint, MessageBrokerStub>();
		services.AddSingleton(Mock.Of<ITransactionExceptionClassifier>());
		services.AddScoped<BatchResource>();
		await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
		var transaction = provider.GetRequiredService<IApplicationTransactionFactory>().Create();
		var context = transaction.Context;
		var scopedTransaction = context.GetRequiredService<IApplicationTransaction>();
		scopedTransaction.Should().BeSameAs(transaction);
		scopedTransaction.Context.Should().BeSameAs(context);
		scopedTransaction.IsExecuting.Should().BeFalse();
		var resource = context.GetRequiredService<BatchResource>();
		scopedTransaction.WithSettings(Attributes.TransactionalAttribute.ReadCommitted(0, 0))
			.Should().BeSameAs(scopedTransaction);
		resource.Disposed.Should().BeFalse();
		await transaction.DisposeAsync();
		resource.Disposed.Should().BeTrue();
		resource.DisposeCalls.Should().Be(1);
		await transaction.DisposeAsync();
		transaction.Dispose();
		resource.DisposeCalls.Should().Be(1);
		var accessDisposedContext = () => transaction.Context;
		accessDisposedContext.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
	public async Task DisposedFactoryTransaction_CannotResolveOrExecuteAgain()
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		await using var provider = services.BuildServiceProvider();
		var transaction = provider.GetRequiredService<IApplicationTransactionFactory>().Create();
		await transaction.DisposeAsync();
		await transaction.DisposeAsync();
		var configure = () => transaction.WithSettings(null);
		configure.Should().Throw<ObjectDisposedException>();
		var registerHandler = () => transaction.OnException(_ => { });
		registerHandler.Should().Throw<ObjectDisposedException>();
		var executed = false;
		await Assert.ThrowsAsync<ObjectDisposedException>(() => transaction.ExecuteAsync((_, _) =>
		{
			executed = true;
			return Task.CompletedTask;
		}, TestContext.Current.CancellationToken));
		executed.Should().BeFalse();
	}

	[Fact]
	public void Factory_ResolutionFailureDisposesTheNewScope()
	{
		var failure = new InvalidOperationException("Cannot resolve the transaction.");
		var services = new Mock<IServiceProvider>();
		services.Setup(provider => provider.GetService(typeof(ApplicationTransaction))).Throws(failure);
		var scope = new Mock<IServiceScope>();
		scope.SetupGet(value => value.ServiceProvider).Returns(services.Object);
		var disposed = false;
		var asyncScope = scope.As<IAsyncDisposable>();
		asyncScope.Setup(value => value.DisposeAsync()).Returns(async () =>
		{
			await Task.Yield();
			disposed = true;
		});
		var scopeFactory = new Mock<IServiceScopeFactory>();
		scopeFactory.Setup(value => value.CreateScope()).Returns(scope.Object);
		var factory = new ApplicationTransactionFactory(scopeFactory.Object);

		var create = () => factory.Create();
		create.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(failure);
		disposed.Should().BeTrue();
		asyncScope.Verify(value => value.DisposeAsync(), Times.Once);
	}

	[Fact]
	public async Task DomainEventHandler_UsesTheFactoryTransactionAndItsContext()
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		services.RegisterIntegrationEventScope();
		services.RegisterDomainEventScope();
		services.AddLogging();
		services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<TransactionProbeHandler>());
		services.AddScoped(_ => Mock.Of<IUnitOfWork>(unit => unit.Context.SuppressAutoSave == true));
		await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
		await using var transaction = provider.GetRequiredService<IApplicationTransactionFactory>().Create();
		var probe = new TransactionProbeEvent(transaction);

		await transaction.ExecuteAsync((context, _) =>
		{
			context.GetRequiredService<IDomainEventScope>().Add(probe);
			return Task.CompletedTask;
		}, TestContext.Current.CancellationToken);

		probe.Handled.Should().BeTrue();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task DisposingBorrowedTransaction_LeavesTheRequestScopeAlive(bool asynchronously)
	{
		var services = new ServiceCollection();
		services.RegisterApplicationTransactions();
		services.AddScoped<BatchResource>();
		await using var provider = services.BuildServiceProvider();
		var scope = provider.CreateAsyncScope();
		var transaction = scope.ServiceProvider.GetRequiredService<IApplicationTransaction>();
		var resource = transaction.Context.GetRequiredService<BatchResource>();

		if (asynchronously) await transaction.DisposeAsync();
		else transaction.Dispose();

		resource.Disposed.Should().BeFalse();
		scope.ServiceProvider.GetRequiredService<BatchResource>().Should().BeSameAs(resource);
		await scope.DisposeAsync();
		resource.DisposeCalls.Should().Be(1);
	}

	private sealed record TransactionProbeEvent(IApplicationTransaction Expected) : IDomainEvent
	{
		public bool Handled { get; set; }
	}

	private sealed class TransactionProbeHandler(IApplicationTransaction transaction)
		: INotificationHandler<TransactionProbeEvent>
	{
		public async Task Handle(TransactionProbeEvent notification, CancellationToken cancellationToken)
		{
			transaction.Should().BeSameAs(notification.Expected);
			transaction.IsExecuting.Should().BeTrue();
			await transaction.ExecuteAsync((context, _) =>
			{
				context.Should().BeSameAs(notification.Expected.Context);
				notification.Handled = true;
				return Task.CompletedTask;
			}, cancellationToken);
		}
	}

	private sealed class BatchResource : IAsyncDisposable
	{
		public bool Disposed => DisposeCalls != 0;
		public int DisposeCalls { get; private set; }
		public async ValueTask DisposeAsync()
		{
			await Task.Yield();
			DisposeCalls++;
		}
	}
}
