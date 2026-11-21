using Application.Common.Dtos;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;
using NamedObject.Core.Interfaces;
using SchemaGeneration.Abstractions;

namespace Main.Modules.Jobs.Application.Queries.Jobs;

public sealed record GetJobInfosQuery : IQuery<GetJobInfosResult>
{
	public GetJobInfosQuery() => SystemNames = null;

	public GetJobInfosQuery(IEnumerable<string> systemNames) => SystemNames = systemNames.Distinct().ToArray();

	public IReadOnlyList<string>? SystemNames { get; }
}

public sealed record GetJobInfosResult(IReadOnlyDictionary<string, JobInfoDto> Jobs);

public sealed class GetJobInfosHandler(
	IContextualLocalizer localizer,
	INamedObjectRegistry<ILrtNamedObject> registry,
	ISchemaGenerator schemaGenerator) : IQueryHandler<GetJobInfosQuery, GetJobInfosResult>
{
	public Task<GetJobInfosResult> Handle(
		GetJobInfosQuery request,
		CancellationToken cancellationToken)
	{
		var lrts = request.SystemNames is null
			? registry.All.Select(lrt => (Key: lrt.SystemName, Lrt: lrt))
			: request.SystemNames
				.Select(name => (Key: name, Lrt: registry.TryGetBySystemName(name)))
				.Where(x => x.Lrt is not null)
				.Select(x => (x.Key, Lrt: x.Lrt!));

		var result = lrts.ToDictionary(
			x => x.Key,
			x => new JobInfoDto
			{
				SystemName = x.Lrt.SystemName,
				Name = localizer.Get(x.Lrt.NameLocalizationMessage),
				Description = localizer.Get(x.Lrt.DescriptionLocalizationMessage),
				InitStateSchema = schemaGenerator.Generate(x.Lrt.InputType)
			});

		return Task.FromResult(new GetJobInfosResult(result));
	}
}
