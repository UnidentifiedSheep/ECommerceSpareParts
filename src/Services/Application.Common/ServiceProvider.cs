using System.Reflection;
using Abstractions.Interfaces;
using Application.Common.Backplane;
using Application.Common.Behaviors;
using Application.Common.Extensions;
using Application.Common.Handlers.Jobs;
using Application.Common.Handlers.Jobs.GetJobs;
using Application.Common.Handlers.JobSchedules;
using Application.Common.Handlers.JobSchedules.CreateSchedule;
using Application.Common.Handlers.JobSchedules.GetSchedule;
using Application.Common.Handlers.JobSchedules.UpdateSchedule;
using Application.Common.NamedObject;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            .RegisterIdCollector()
            .RegisterIntegrationEventScope()
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
                    typeof(TransactionBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(SaveChangesBehavior<,>),
                    ServiceLifetime.Scoped)
                .RegisterIfNotExcluded(
                    hs,
                    typeof(IntegrationEventPublisherBehavior<,>),
                    ServiceLifetime.Scoped);
        });

        return services;
    }

    public static IServiceCollection AddLrtLayer(
        this IServiceCollection services,
        Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        services.RegisterNamedObject<LrtNamedObjectBase>(assembly)
            .RegisterFluentValidations(typeof(GetAllAvailableJobsHandler).Assembly);

        services.AddScoped<
            IRequestHandler<GetAllAvailableJobsQuery, GetAllAvailableJobsResult>,
            GetAllAvailableJobsHandler>();

        services.AddScoped<
            IRequestHandler<QueueJobCommand, QueueJobResult>,
            QueueJobHandler>();

        services.AddScoped<
            IRequestHandler<RunJobBatchCommand, Unit>,
            RunJobBatchHandler>();

        services.AddScoped<
            IRequestHandler<GetJobsQuery, GetJobsResult>,
            GetJobsHandler>();

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
            IRequestHandler<UpdateScheduleCommand, UpdateScheduleResult>,
            UpdateScheduleHandler>();

        services.AddScoped<
            IRequestHandler<QueueScheduledJobsCommand, Unit>,
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