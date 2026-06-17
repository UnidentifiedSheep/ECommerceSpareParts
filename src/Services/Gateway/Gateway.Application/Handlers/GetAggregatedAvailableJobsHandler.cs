using Abstractions;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Gateway.Application.Dtos;
using Internal.Integration.Core.Interfaces;
using Localization.Abstractions.Models;

namespace Gateway.Application.Handlers;

[Diagnostics(true, 500)]
public record GetAggregatedAvailableJobsQuery(
    Locale Locale
    ) : IQuery<GetAggregatedAvailableJobsResult>;
public record GetAggregatedAvailableJobsResult(ServiceJobsDto[] Jobs);

public class GetAggregatedAvailableJobsHandler(
    ICommonClient commonClient
    ) : IQueryHandler<GetAggregatedAvailableJobsQuery, GetAggregatedAvailableJobsResult>
{
    private static readonly IServiceDefinition[] Services =
    [
        ServicesDefinitions.Main,
        ServicesDefinitions.Analytics,
        ServicesDefinitions.Pricing,
        ServicesDefinitions.Search
    ];
    
    public async Task<GetAggregatedAvailableJobsResult> Handle(
        GetAggregatedAvailableJobsQuery request, 
        CancellationToken cancellationToken)
    {
        var tasks = Services.Select(service => GetJobsForAsync(service, request.Locale, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return new GetAggregatedAvailableJobsResult(results);
    }

    private async Task<ServiceJobsDto> GetJobsForAsync(
        IServiceDefinition serviceDefinition, 
        Locale locale,
        CancellationToken token)
    {
        try
        {
            var result = await commonClient.GetAvailableJobs(
                serviceDefinition,
                locale,
                token);

            return new ServiceJobsDto
            {
                Available = true,
                Jobs = result.Select(x => new GatewayJobInfoDto
                {
                    SystemName = x.SystemName,
                    Name = x.Name,
                    InitStateSchema = x.InitStateSchema,
                    Description = x.Description
                }).ToList(),
                ServiceName = serviceDefinition.ServiceName
            };
        }
        catch (Exception)
        {
            return new ServiceJobsDto
            {
                Available = false,
                Jobs = [],
                ServiceName = serviceDefinition.ServiceName
            };
        }
    }
}