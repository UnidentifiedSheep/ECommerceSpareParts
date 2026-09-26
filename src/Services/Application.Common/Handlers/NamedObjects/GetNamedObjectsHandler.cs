using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Locan.Core.Interfaces.Localizers;
using NamedObject.Core.Base;
using NamedObject.Core.Interfaces;

namespace Application.Common.Handlers.NamedObjects;

public record GetNamedObjectsQuery(string GroupName) : IQuery<GetNamedObjectsResult>;

public record GetNamedObjectsResult(IReadOnlyList<NamedObjectDto> NamedObjects);

public class GetNamedObjectsHandler(
	INamedObjectGroupResolver resolver,
	IContextualLocalizer localizer)
	: IQueryHandler<GetNamedObjectsQuery, GetNamedObjectsResult>
{
	public Task<GetNamedObjectsResult> Handle(
		GetNamedObjectsQuery request,
		CancellationToken cancellationToken)
	{
		var resolved = resolver
			.GetByGroupName(request.GroupName)
			.All
			.Select(x =>
			{
				string? name = null;
				string? description = null;

				if (x is LocalizableNameObject loc)
				{
					name = localizer.Get(loc.NameLocalizationMessage);
					description = localizer.Get(loc.DescriptionLocalizationMessage);
				}

				return new NamedObjectDto
				{
					SystemName = x.SystemName,
					Name = name,
					Description = description
				};
			})
			.ToList();

		return Task.FromResult(new GetNamedObjectsResult(resolved));
	}
}
