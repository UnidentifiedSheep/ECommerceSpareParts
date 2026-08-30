using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;
using SchemaGeneration.Abstractions;

namespace Application.Common.Handlers.Jobs;

public sealed record GetAllAvailableJobsQuery : IQuery<GetAllAvailableJobsResult>;

public sealed record GetAllAvailableJobsResult(IReadOnlyList<JobInfoDto> Jobs);

public sealed class GetAllAvailableJobsHandler(
	IContextualStringLocalizer localizer,
	INamedObjectRegistry<ILrtNamedObject> registry,
	ISchemaGenerator schemaGenerator) : IQueryHandler<GetAllAvailableJobsQuery, GetAllAvailableJobsResult>
{
	public Task<GetAllAvailableJobsResult> Handle(
		GetAllAvailableJobsQuery request,
		CancellationToken cancellationToken)
	{
		var result = registry
			.All
			.Select(x => new JobInfoDto
			{
				SystemName = x.SystemName,
				Name = localizer.Get(x.NameLocalizationKey),
				Description = localizer.Get(x.DescriptionLocalizationKey),
				InitStateSchema = schemaGenerator.Generate(x.InputType)
			})
			.ToList();

		return Task.FromResult(new GetAllAvailableJobsResult(result));
	}
}
