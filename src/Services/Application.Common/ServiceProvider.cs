using System.Reflection;
using Abstractions.Interfaces;
using Application.Common.Backplane;
using Application.Common.Behaviors;
using Application.Common.DomainEventHandlers.Jobs;
using Application.Common.Extensions;
using Application.Common.Handlers.Jobs;
using Application.Common.Handlers.Jobs.GetJobs;
using Application.Common.Handlers.JobSchedules;
using Application.Common.Handlers.JobSchedules.CreateSchedule;
using Application.Common.Handlers.JobSchedules.GetSchedule;
using Application.Common.Handlers.JobSchedules.UpdateSchedule;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Application.Common.Projections;
using Application.Common.Services;
using Application.Common.Services.Events;
using Application.Common.Services.Job;
using Domain.CommonEntities.Job.Events;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchemaGeneration.Extensions;
using ZiggyCreatures.Caching.Fusion.Backplane;

namespace Application.Common;

public static class ServiceProvider
{
    public static IServiceCollection AddApplicationBase(
        this IServiceCollection services,
        IServiceDefinition serviceDefinition,
        IConfiguration? configuration,
        Assembly? assembly = null,
        params Type[] behaviorsToExclude)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services
            .AddSchemaGeneration()
            .AddCqrsMetrics()
            .RegisterIdCollector()
            .RegisterIntegrationEventScope()
            .RegisterDomainEventScope()
            .RegisterCachePolicies(assembly)
            .RegisterDbValidations(assembly)
            .RegisterFluentValidations(assembly); 

        services.AddSingleton<IBackplaneDispatcher, BackplaneDispatcher>();
        services.AddSingleton<IFusionCacheBackplane, MassTransitBackplane>();
        services.AddSingleton(serviceDefinition);

        var hs = behaviorsToExclude.ToHashSet();
        services.AddMediatR(config =>
        {
            var licenseKey = configuration?.GetValue<string>("MediatR:LicenseKey");
            if (!string.IsNullOrWhiteSpace(licenseKey)) config.LicenseKey = licenseKey;

            config.RegisterServicesFromAssembly(assembly);
            config
                .RegisterIfNotExcluded(
                    hs,
                    typeof(MetricsBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(DiagnosticsBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(ValidationBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(DbValidationBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(CacheBehavior<,>))
                .RegisterIfNotExcluded(
                    hs,
                    typeof(ApplicationTransactionBehavior<,>),
                    ServiceLifetime.Scoped);
        });

        return services;
    }

    public static IServiceCollection AddLrtLayer(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services.RegisterNamedObject<ILrtNamedObject>(assembly)
            .RegisterFluentValidations(typeof(GetAllAvailableJobsHandler).Assembly)
            .RegisterProjectionProviders<JobDtoProjectionProvider>();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IOperationDatePolicy, OperationDatePolicy>();

        services.AddScoped<IJobLeaseService, JobLeaseService>();
        services.AddScoped<IJobCreationDispatcher, JobCreationDispatcher>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobScheduleService, JobScheduleService>();
        services.AddSingleton<ILrtQuotaManager, LrtQuotaManager>();
        services.AddScoped<
            INotificationHandler<Batch<JobStepFinishedDomainEvent>>,
            ResumeMultiStepJobHandler>();
        services.AddScoped<
            INotificationHandler<Batch<JobStatusUpdatedDomainEvent>>,
            PublishJobStatusUpdatedEventHandler>();
        
        services.AddScoped<
            IRequestHandler<GetAllAvailableJobsQuery, GetAllAvailableJobsResult>,
            GetAllAvailableJobsHandler>();

        services.AddScoped<
            IRequestHandler<QueueJobCommand, QueueJobResult>,
            QueueJobHandler>();
        
        services.AddScoped<
            IRequestHandler<CancelJobCommand, Unit>,
            CancelJobHandler>();

        services.AddScoped<
            IRequestHandler<GetJobsQuery, GetJobsResult>,
            GetJobsHandler>();

        services.AddScoped<
            IRequestHandler<GetJobQuery, GetJobResult>,
            GetJobHandler>();

        services.AddScoped<
            IRequestHandler<GetJobStateQuery, GetJobStateResult>,
            GetJobStateHandler>();

        services.AddScoped<
            IRequestHandler<CreateScheduleCommand, CreateScheduleResult>,
            CreateScheduleHandler>();

        services.AddScoped<
            IRequestHandler<GetScheduleQuery, GetScheduleResult>,
            GetScheduleHandler>();

        services.AddScoped<
            IRequestHandler<GetScheduleByIdQuery, GetScheduleByIdResult>,
            GetScheduleByIdHandler>();

        services.AddScoped<
            IRequestHandler<UpdateScheduleCommand, UpdateScheduleResult>,
            UpdateScheduleHandler>();

        services.AddScoped<
            IRequestHandler<QueueScheduledJobsCommand, QueueScheduledJobsResult>,
            QueueScheduledJobsHandler>();
        
        services.AddScoped<
            IRequestHandler<RemoveJobScheduleCommand, Unit>,
            RemoveJobScheduleHandler>();

        return services;
    }

    private static MediatRServiceConfiguration RegisterIfNotExcluded(
        this MediatRServiceConfiguration serviceConfiguration,
        HashSet<Type> excludedTypes,
        Type openBehaviorType,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (excludedTypes.Contains(openBehaviorType)) return serviceConfiguration;
        serviceConfiguration.AddOpenBehavior(openBehaviorType, serviceLifetime);
        return serviceConfiguration;
    }
}
