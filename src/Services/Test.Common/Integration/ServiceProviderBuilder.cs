using Abstractions.Interfaces;
using Application.Common;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Localization.Domain.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Common;
using Persistence.Extensions;
using Persistence.Interceptors;
using Tests.Abstractions.Test;
using Tests.Extensions;
using Tests.Interfaces.ServiceProvider;
using Tests.Persistence.Context;
using Tests.Persistence.Repositories;
using Tests.Stubs;
using Tests.TestContexts;
using ApplicationServiceProvider = Application.Common.ServiceProvider;

namespace Tests.Integration;

internal sealed class ServiceProviderBuilder : IServiceProviderBuilder<ServiceProviderArguments>
{
	public IServiceProvider Build(ServiceProviderArguments args)
	{
		RegisterGlobalBasicContexts();
		var services = new ServiceCollection();
		services.RegisterTestContexts();
		services.AddLogging();

		services
			.AddApplicationBase(
				new CommonTestServiceDefinition(),
				null,
				typeof(ApplicationServiceProvider).Assembly,
				typeof(CacheBehavior<,>),
				typeof(DbValidationBehavior<,>))
			.AddLrtLayer()
			.AddLocalization(
				"ru-RU",
				"ru-RU",
				"en-EN")
			.RegisterSettingsService();

		services.AddScoped<AuditableEntitySaveChangesInterceptor>();
		services.AddScoped<DomainEventFlusherSaveChangesInterceptor>();
		services.AddDbContext<DContext>((sp, options) =>
		{
			options.UseNpgsql(args.PgsqlConnectionString);
			options.AddInterceptors(
				sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
				sp.GetRequiredService<DomainEventFlusherSaveChangesInterceptor>());
		});

		services.AddJobRepositories<DContext>();
		services.AddScoped(typeof(IRepository<,>), typeof(BasicEfRepository<,>));
		services.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
		services.AddUnitOfWork<DContext>();
		services.AddScoped<IUserContext, UserContextMock>();
		services.AddScoped<MessageBrokerStub>();
		services.AddScoped<IPublishEndpoint>(sp => sp.GetRequiredService<MessageBrokerStub>());
		services.AddSingleton<ILrtNamedObject, JobScheduleTestLrt>();
		services.AddSingleton<TestTimeProvider>();
		services.Replace(
			ServiceDescriptor.Singleton<TimeProvider>(sp => sp.GetRequiredService<TestTimeProvider>()));

		return services.BuildServiceProvider();
	}

	private static void RegisterGlobalBasicContexts() =>
		TestBase.RegisterGlobalBasicContext<LocalizedTestContext>();

	private sealed class CommonTestServiceDefinition : IServiceDefinition
	{
		public string ServiceName => "application-common-tests";
	}
}
