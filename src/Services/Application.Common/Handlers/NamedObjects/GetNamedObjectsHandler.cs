using Application.Common.Abstractions.NamedObjects;
using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Handlers.NamedObjects;

public record GetNamedObjectsQuery(
    string GroupName) : IQuery<GetNamedObjectsResult>;

public record GetNamedObjectsResult(IReadOnlyList<NamedObjectDto> NamedObjects);

public class GetNamedObjectsHandler(
    INamedObjectGroupResolver resolver,
    IScopedStringLocalizer localizer
    ) : IQueryHandler<GetNamedObjectsQuery, GetNamedObjectsResult>
{
    public Task<GetNamedObjectsResult> Handle(GetNamedObjectsQuery request, CancellationToken cancellationToken)
    {
        var resolved = resolver.GetByGroupName(request.GroupName).All
            .Select(x =>
            {
                string? name = null;
                string? description = null;
                
                if (x is LocalizableNameObject loc)
                {
                    name = localizer[loc.NameLocalizationKey];
                    description = localizer[loc.DescriptionLocalizationKey];
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