using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Locan.Core.Interfaces.Localizers;
using NamedObject.Core.Interfaces;
using SchemaGeneration.Abstractions;

namespace Application.Common.Handlers.Jobs;

public sealed record GetAllAvailableJobsQuery : IQuery<GetAllAvailableJobsResult>;

public sealed record GetAllAvailableJobsResult(IReadOnlyList<JobInfoDto> Jobs);

public sealed class GetAllAvailableJobsHandler(
	IContextualLocalizer localizer,
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
				Name = localizer.Get(x.NameLocalizationMessage),
				Description = localizer.Get(x.DescriptionLocalizationMessage),
				InitStateSchema = schemaGenerator.Generate(x.InputType)
			})
			.ToList();

		return Task.FromResult(new GetAllAvailableJobsResult(result));
	}
}
