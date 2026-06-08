using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Handlers.Jobs;

public sealed record GetAllAvailableJobsQuery() : IQuery<GetAllAvailableJobsResult>;
public sealed record GetAllAvailableJobsResult(IReadOnlyList<JobDto> Jobs);

public sealed class GetAllAvailableJobsHandler(
    IScopedStringLocalizer localizer,
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IScopedLocalizedJsonSerializer jsonSerializer
    ) : IQueryHandler<GetAllAvailableJobsQuery, GetAllAvailableJobsResult>
{
    public Task<GetAllAvailableJobsResult> Handle(GetAllAvailableJobsQuery request, CancellationToken cancellationToken)
    {
        var result = registry.All
            .Select(x => new JobDto
            {
                SystemName = x.SystemName,
                Name = localizer.Get(x.NameLocalizationKey),
                Description = localizer.Get(x.DescriptionLocalizationKey),
                InitStateSchema = jsonSerializer.SerializeMetadata(x.InputType)
            }).ToList();

        return Task.FromResult(new GetAllAvailableJobsResult(result));
    }
}