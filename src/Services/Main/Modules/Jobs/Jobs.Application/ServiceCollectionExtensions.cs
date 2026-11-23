using Application.Common.Extensions;
using Application.Common.Host;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.Services;
using Main.Modules.Jobs.Application.Configs;
using Main.Modules.Jobs.Application.Domains;
using Main.Modules.Jobs.Application.HostedServices;
using Main.Modules.Jobs.Application.LRT;
using Main.Modules.Jobs.Application.Models.Options;
using Main.Modules.Jobs.Application.Services.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NamedObject;
using NamedObject.Core.Interfaces;
using Persistence.Listening;

namespace Main.Modules.Jobs.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddJobsApplication(this IServiceCollection services)
	{
		JobsSortByConfig.Configure();

		var assembly = JobsApplicationAssembly.Value;

		services.AddCommonDomain<JobsDomain>();
		services
			.AddModuleApplication(assembly)
			.RegisterNamedObject<ILrtNamedObject>(assembly);

		services.TryAddSingleton(TimeProvider.System);
		services.TryAddScoped<IOperationDatePolicy, OperationDatePolicy>();

		services.AddScoped<IJobLeaseService, JobLeaseService>();
		services.AddScoped<IJobCreationDispatcher, JobCreationDispatcher>();
		services.AddScoped<IJobStepInputInitializer, JobStepInputInitializer>();
		services.AddScoped<Func<IJobCreationDispatcher>>(serviceProvider =>
			serviceProvider.GetRequiredService<IJobCreationDispatcher>);
		services.AddScoped<IJobService, JobService>();
		services.AddScoped<IJobScheduleService, JobScheduleService>();
		services.AddDefaultLrtQuotas();
		services.AddSingleton<LrtQuotaRegistry>();
		services.AddSingleton<INamedObjectRegistry<ILrtQuotaManager>>(provider
			=> provider.GetRequiredService<LrtQuotaRegistry>());
		services.AddSingleton<ILrtQuotaCatalog, LrtQuotaCatalog>();
		services.AddScoped<ILrtJobAcquirer, LrtJobAcquirer>();

		return services;
	}

	public static IServiceCollection AddJobsOptions(this IServiceCollection collection)
	{
		collection
			.AddOptions<LrtExecutorOptions>()
			.BindConfiguration(LrtExecutorOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		collection
			.AddOptions<ScheduledJobEnqueuerOptions>()
			.BindConfiguration(ScheduledJobEnqueuerOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return collection;
	}

	public static IServiceCollection AddJobsHostedServices(this IServiceCollection serviceCollection)
		=> serviceCollection
			.AddChannelListening()
			.AddSingleton<IJobSignal, JobSignal>()
			.AddHostedService<JobNotificationListenerHostedService>()
			.AddHostedService<LrtExecutorHostedService>()
			.AddHostedService<ScheduledJobEnqueuerHostedService>()
			.AddHostedService<ExpiredJobsWiperHostedService>();
}
